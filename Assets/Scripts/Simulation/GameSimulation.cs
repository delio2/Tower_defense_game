using System;
using System.Collections.Generic;

namespace TowerDefense.Simulation
{
    public enum GamePhase : byte
    {
        Shop = 0,
        Wave = 1,
        Victory = 2,
        Defeat = 3,
    }

    /// <summary>
    /// Deterministic core-defense simulation (GDD v0.2). Time advances only during waves, at 60 ticks per second;
    /// the shop is timeless. Same seed + same (tick, command) log always produces the same state (D11, D19).
    /// </summary>
    public sealed class GameSimulation
    {
        private const int NoOffer = -1;

        private readonly RunConfig _config;
        private readonly ContentDatabase _content;
        private readonly WaveDirector _waves;
        private readonly Pcg32 _shopRng;

        private readonly List<Enemy> _enemies = new List<Enemy>();
        private readonly List<Enemy> _pendingSpawns = new List<Enemy>();
        private readonly List<Enemy> _targets = new List<Enemy>();
        private readonly List<SimEvent> _events = new List<SimEvent>();
        private readonly Queue<Command> _pending = new Queue<Command>();
        private readonly List<(long Tick, Command Command)> _commandLog = new List<(long, Command)>();
        private readonly int[] _offers;
        private readonly Stack<ShopSnapshot> _undo = new Stack<ShopSnapshot>();
        private List<SpawnEntry> _currentSpawns = new List<SpawnEntry>();
        private List<SpawnEntry> _nextWave;

        private int _nextModuleId = 1;
        private int _nextEnemyId = 1;
        private int _spawnCursor;
        private long _waveStartTick;
        private int _killsAtWaveStart;

        public RunConfig Config => _config;
        public ContentDatabase Content => _content;
        public Ring Ring { get; }
        public long Tick { get; private set; }
        public GamePhase Phase { get; private set; } = GamePhase.Shop;
        public int Credits { get; private set; }
        public long Integrity { get; private set; }
        public long MaxIntegrity { get; private set; }
        public int WavesCleared { get; private set; }
        public int RerollCost { get; private set; }
        public int PulseCooldownRemaining { get; private set; }
        public long TotalDamage { get; private set; }
        public int Kills { get; private set; }

        /// <summary>The enemy kind whose contact ended the run, or null while the run is not lost (for balance reports).</summary>
        public EnemyKind? DefeatedBy { get; private set; }

        public int TotalWaves => _config.TotalWaves;

        /// <summary>1-based index of the wave that is running or will run next (unbounded in Endless).</summary>
        public int CurrentWave => _config.Endless ? WavesCleared + 1 : Math.Min(WavesCleared + 1, TotalWaves);

        /// <summary>True once the last act is cleared (also in Endless, where play goes on).</summary>
        public bool HasWon => WavesCleared >= TotalWaves;

        public bool IsOver => Phase == GamePhase.Victory || Phase == GamePhase.Defeat;
        public bool IsPulseReady => Phase == GamePhase.Wave && PulseCooldownRemaining == 0;
        public bool IsGuardianWave => _waves.IsGuardianWave(CurrentWave);
        public bool CanUndo => Phase == GamePhase.Shop && _undo.Count > 0;

        /// <summary>The extra slot is on sale once the first Guardian is beaten, until the ring has 8 slots (GDD v0.2 §5).</summary>
        public bool IsExtraSlotUnlocked => WavesCleared >= _config.WavesPerAct;
        public bool CanBuyExtraSlot => Phase == GamePhase.Shop && IsExtraSlotUnlocked && Ring.SlotCount < Ring.MaxSlots;
        public int ExtraSlotCost => _config.ExtraSlotCost;

        public IReadOnlyList<Enemy> Enemies => _enemies;
        public IReadOnlyList<(long Tick, Command Command)> CommandLog => _commandLog;

        /// <summary>Spawn plan of the next wave, shown in the shop (GDD v0.2 §10).</summary>
        public IReadOnlyList<SpawnEntry> NextWavePreview =>
            _nextWave ?? (IReadOnlyList<SpawnEntry>)Array.Empty<SpawnEntry>();

        public GameSimulation(RunConfig config, ContentDatabase content)
        {
            _config = config;
            _content = content;
            _waves = new WaveDirector(config, content, Pcg32.ForStream(config.Seed, RngStream.Waves));
            _shopRng = Pcg32.ForStream(config.Seed, RngStream.Shop);
            _offers = new int[config.ShopOffers];

            Ring = new Ring(config.StartingSlots);
            Credits = config.StartingCredits;
            MaxIntegrity = config.BaseIntegrity;
            Integrity = MaxIntegrity;

            for (int i = 0; i < config.StartingModules.Length; i++)
            {
                ModuleDefinition definition = content.Module(config.StartingModules[i]);
                Ring.Place(new ModuleInstance(_nextModuleId++, definition, i, definition.Cost), i);
            }

            RecalculateRing();
            OpenShop();
        }

        // ---------------------------------------------------------------- public API

        public void Enqueue(Command command) => _pending.Enqueue(command);

        /// <summary>
        /// Applies queued commands at the current tick without advancing time. Equivalent to applying them at the
        /// start of the next <see cref="Step"/>, so determinism is preserved (shop actions, instant UI feedback).
        /// </summary>
        public void ApplyPendingCommandsNow() => ApplyPendingCommands();

        public void DrainEvents(List<SimEvent> target)
        {
            target.AddRange(_events);
            _events.Clear();
        }

        /// <summary>The module kind offered at <paramref name="index"/>, or null if already bought.</summary>
        public ModuleKind? OfferAt(int index)
        {
            if (index < 0 || index >= _offers.Length || _offers[index] == NoOffer)
            {
                return null;
            }

            return (ModuleKind)_offers[index];
        }

        public int OfferCount => _offers.Length;

        /// <summary>Checks a command without applying it (UI previews).</summary>
        public CommandResult Validate(Command command)
        {
            if (IsOver)
            {
                return CommandResult.GameOver;
            }

            switch (command.Type)
            {
                case CommandType.Buy:
                    return ValidateBuy(command.A, command.B, out _, out _);
                case CommandType.Sell:
                    if (Phase != GamePhase.Shop)
                    {
                        return CommandResult.OnlyInShop;
                    }

                    return Ring.At(command.A) != null ? CommandResult.Ok : CommandResult.SlotEmpty;
                case CommandType.Move:
                    if (Phase != GamePhase.Shop)
                    {
                        return CommandResult.OnlyInShop;
                    }

                    if (!Ring.IsValidSlot(command.A) || !Ring.IsValidSlot(command.B))
                    {
                        return CommandResult.InvalidSlot;
                    }

                    return Ring.At(command.A) != null ? CommandResult.Ok : CommandResult.SlotEmpty;
                case CommandType.Reroll:
                    if (Phase != GamePhase.Shop)
                    {
                        return CommandResult.OnlyInShop;
                    }

                    return Credits >= RerollCost ? CommandResult.Ok : CommandResult.NotEnoughCredits;
                case CommandType.StartWave:
                    return Phase == GamePhase.Shop ? CommandResult.Ok : CommandResult.OnlyInShop;
                case CommandType.Undo:
                    if (Phase != GamePhase.Shop)
                    {
                        return CommandResult.OnlyInShop;
                    }

                    return _undo.Count > 0 ? CommandResult.Ok : CommandResult.NothingToUndo;
                case CommandType.Pulse:
                    if (Phase != GamePhase.Wave)
                    {
                        return CommandResult.OnlyDuringWave;
                    }

                    return PulseCooldownRemaining == 0 ? CommandResult.Ok : CommandResult.PulseNotReady;
                case CommandType.BuySlot:
                    return ValidateBuySlot(command.A);
                default:
                    return CommandResult.Ok;
            }
        }

        /// <summary>Advances one tick. Does nothing (and keeps the tick) outside of waves.</summary>
        public void Step()
        {
            ApplyPendingCommands();
            if (Phase != GamePhase.Wave)
            {
                return;
            }

            SpawnDueEnemies();
            MoveEnemies();
            FireModules();
            AddPendingSpawns();
            RemoveFinishedEnemies();
            if (PulseCooldownRemaining > 0)
            {
                PulseCooldownRemaining--;
            }

            Tick++;
            CheckWaveEnd();
        }

        public ulong ComputeStateHash()
        {
            var hasher = new StateHasher();
            hasher.Add(Tick);
            hasher.Add((long)Phase);
            hasher.Add(Credits);
            hasher.Add(Integrity);
            hasher.Add(MaxIntegrity);
            hasher.Add(WavesCleared);
            hasher.Add(RerollCost);
            hasher.Add(PulseCooldownRemaining);
            hasher.Add(TotalDamage);
            hasher.Add(Kills);
            hasher.Add(_undo.Count);
            hasher.Add(Ring.SlotCount);
            foreach (int offer in _offers)
            {
                hasher.Add(offer);
            }

            for (int slot = 0; slot < Ring.SlotCount; slot++)
            {
                ModuleInstance module = Ring.At(slot);
                hasher.Add(module == null ? -1 : (long)module.Kind);
                hasher.Add(module?.Level ?? 0);
                hasher.Add(module?.Invested ?? 0);
                hasher.Add(module?.CooldownRemaining ?? 0);
            }

            foreach (Enemy enemy in _enemies)
            {
                hasher.Add(enemy.Id);
                hasher.Add((long)enemy.Kind);
                hasher.Add(enemy.Direction);
                hasher.Add(enemy.Radius);
                hasher.Add(enemy.Hp);
                hasher.Add(enemy.IsElite ? 1 : 0);
                hasher.Add(enemy.AgeTicks);
            }

            return hasher.Value;
        }

        // ---------------------------------------------------------------- commands

        private void ApplyPendingCommands()
        {
            while (_pending.Count > 0)
            {
                Command command = _pending.Dequeue();
                _commandLog.Add((Tick, command));
                CommandResult result = Apply(command);
                if (result != CommandResult.Ok)
                {
                    _events.Add(new SimEvent(SimEventType.CommandRejected, Tick, extra: (int)result));
                }
            }
        }

        private CommandResult Apply(Command command)
        {
            CommandResult result = Validate(command);
            if (result != CommandResult.Ok)
            {
                return result;
            }

            switch (command.Type)
            {
                case CommandType.Buy:
                    _undo.Push(TakeSnapshot());
                    ApplyBuy(command.A, command.B);
                    break;
                case CommandType.Sell:
                    _undo.Push(TakeSnapshot());
                    ApplySell(command.A);
                    break;
                case CommandType.Move:
                    _undo.Push(TakeSnapshot());
                    Ring.Swap(command.A, command.B);
                    RecalculateRing();
                    _events.Add(new SimEvent(SimEventType.ModulesSwapped, Tick, command.A, extra: command.B));
                    break;
                case CommandType.Undo:
                    RestoreSnapshot(_undo.Pop());
                    _events.Add(new SimEvent(SimEventType.Undone, Tick));
                    break;
                case CommandType.Reroll:
                    // A reroll cannot be undone, otherwise future offers could be peeked for free.
                    _undo.Clear();
                    Credits -= RerollCost;
                    RerollCost++;
                    GenerateOffers();
                    _events.Add(new SimEvent(SimEventType.Rerolled, Tick));
                    break;
                case CommandType.StartWave:
                    StartWave();
                    break;
                case CommandType.Pulse:
                    UsePulse();
                    break;
                case CommandType.BuySlot:
                    _undo.Push(TakeSnapshot());
                    Credits -= _config.ExtraSlotCost;
                    Ring.InsertSlot(command.A);
                    RecalculateRing();
                    _events.Add(new SimEvent(SimEventType.SlotAdded, Tick, value: Ring.SlotCount, extra: command.A));
                    break;
            }

            return CommandResult.Ok;
        }

        private CommandResult ValidateBuySlot(int insertAt)
        {
            if (Phase != GamePhase.Shop)
            {
                return CommandResult.OnlyInShop;
            }

            if (!IsExtraSlotUnlocked)
            {
                return CommandResult.SlotNotUnlocked;
            }

            if (Ring.SlotCount >= Ring.MaxSlots)
            {
                return CommandResult.SlotLimitReached;
            }

            if (insertAt < 0 || insertAt > Ring.SlotCount)
            {
                return CommandResult.InvalidSlot;
            }

            return Credits >= _config.ExtraSlotCost ? CommandResult.Ok : CommandResult.NotEnoughCredits;
        }

        private CommandResult ValidateBuy(int offerIndex, int slot, out ModuleDefinition definition, out ModuleInstance mergeTarget)
        {
            definition = null;
            mergeTarget = null;
            if (Phase != GamePhase.Shop)
            {
                return CommandResult.OnlyInShop;
            }

            if (offerIndex < 0 || offerIndex >= _offers.Length)
            {
                return CommandResult.InvalidOffer;
            }

            if (_offers[offerIndex] == NoOffer)
            {
                return CommandResult.OfferAlreadyBought;
            }

            definition = _content.Module((ModuleKind)_offers[offerIndex]);
            if (Credits < definition.Cost)
            {
                return CommandResult.NotEnoughCredits;
            }

            mergeTarget = Ring.FindMergeTarget(definition.Kind);
            if (mergeTarget != null)
            {
                return CommandResult.Ok;
            }

            if (!Ring.IsValidSlot(slot))
            {
                return CommandResult.InvalidSlot;
            }

            return Ring.At(slot) == null ? CommandResult.Ok : CommandResult.SlotOccupied;
        }

        private void ApplyBuy(int offerIndex, int slot)
        {
            ValidateBuy(offerIndex, slot, out ModuleDefinition definition, out ModuleInstance mergeTarget);
            Credits -= definition.Cost;
            _offers[offerIndex] = NoOffer;

            if (mergeTarget != null)
            {
                mergeTarget.Level++;
                mergeTarget.Invested += definition.Cost;
                RecalculateRing();
                _events.Add(new SimEvent(SimEventType.ModuleMerged, Tick, mergeTarget.Id, mergeTarget.Level, mergeTarget.Slot));
                return;
            }

            var module = new ModuleInstance(_nextModuleId++, definition, slot, definition.Cost);
            Ring.Place(module, slot);
            RecalculateRing();
            _events.Add(new SimEvent(SimEventType.ModuleBought, Tick, module.Id, (long)definition.Kind, slot));
        }

        private void ApplySell(int slot)
        {
            ModuleInstance module = Ring.At(slot);
            int refund = ModuleRules.SellValue(module.Invested);
            Credits += refund;
            Ring.Remove(slot);
            RecalculateRing();
            _events.Add(new SimEvent(SimEventType.ModuleSold, Tick, module.Id, refund, slot));
        }

        // ---------------------------------------------------------------- shop and waves

        private void OpenShop()
        {
            _undo.Clear();
            Phase = GamePhase.Shop;
            RerollCost = _config.FirstRerollCost;
            GenerateOffers();
            _nextWave = _waves.BuildWave(CurrentWave);
            _events.Add(new SimEvent(SimEventType.ShopOpened, Tick, value: CurrentWave));
        }

        /// <summary>Draws offers from the shop stream. Rarity weights: Common 60, Uncommon 30, Rare 10 (Rare from act 2).</summary>
        private void GenerateOffers()
        {
            IReadOnlyList<ModuleKind> pool = _content.ShopPool;
            int act = (CurrentWave - 1) / _config.WavesPerAct + 1;
            for (int i = 0; i < _offers.Length; i++)
            {
                int roll = _shopRng.NextInt(100);
                Rarity rarity = roll < 60 ? Rarity.Common : roll < 90 || act < 2 ? Rarity.Uncommon : Rarity.Rare;
                _offers[i] = (int)PickFromPool(pool, rarity);
            }
        }

        private ModuleKind PickFromPool(IReadOnlyList<ModuleKind> pool, Rarity rarity)
        {
            int count = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                if (_content.Module(pool[i]).Rarity == rarity)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return pool[_shopRng.NextInt(pool.Count)];
            }

            int pick = _shopRng.NextInt(count);
            for (int i = 0; i < pool.Count; i++)
            {
                if (_content.Module(pool[i]).Rarity != rarity)
                {
                    continue;
                }

                if (pick-- == 0)
                {
                    return pool[i];
                }
            }

            return pool[0];
        }

        private void StartWave()
        {
            _undo.Clear();
            _currentSpawns = _nextWave;
            _nextWave = null;
            _spawnCursor = 0;
            _waveStartTick = Tick;
            _killsAtWaveStart = Kills;
            PulseCooldownRemaining = 0;
            Phase = GamePhase.Wave;
            for (int slot = 0; slot < Ring.SlotCount; slot++)
            {
                ModuleInstance module = Ring.At(slot);
                if (module != null)
                {
                    module.CooldownRemaining = 0;
                }
            }

            _events.Add(new SimEvent(SimEventType.WaveStarted, Tick, value: CurrentWave));
        }

        private void SpawnDueEnemies()
        {
            while (_spawnCursor < _currentSpawns.Count && _waveStartTick + _currentSpawns[_spawnCursor].OffsetTicks <= Tick)
            {
                SpawnEntry entry = _currentSpawns[_spawnCursor++];
                SpawnEnemy(entry.Kind, entry.Direction, SimConstants.SpawnRadius, _enemies, entry.IsElite);
            }
        }

        /// <summary>Test hook: places an enemy during a wave at an exact direction and radius (micro-units).</summary>
        internal Enemy SpawnForTests(EnemyKind kind, int direction, long radius, bool elite = false)
        {
            return SpawnEnemy(kind, direction, radius, _enemies, elite);
        }

        private Enemy SpawnEnemy(EnemyKind kind, int direction, long radius, List<Enemy> into, bool elite = false)
        {
            EnemyDefinition definition = _content.Enemy(kind);
            var enemy = new Enemy(_nextEnemyId++, definition, _waves.EnemyMaxHp(definition, CurrentWave), direction, radius, elite);
            into.Add(enemy);
            _events.Add(new SimEvent(SimEventType.EnemySpawned, Tick, enemy.Id, (long)kind, enemy.Direction));
            return enemy;
        }

        private void CheckWaveEnd()
        {
            if (Phase != GamePhase.Wave || _spawnCursor < _currentSpawns.Count || _enemies.Count > 0)
            {
                return;
            }

            bool guardianWave = _waves.IsGuardianWave(CurrentWave);
            int interestCap = _config.InterestCap + Ring.InterestCapBonus;
            int interest = Math.Min(interestCap, Credits / _config.InterestStep);
            int salvage = (Kills - _killsAtWaveStart) * Ring.SalvagePermillePerKill / SimConstants.Permille;
            Credits += _config.CreditsPerWave + interest + Ring.CreditsPerWave + salvage + (guardianWave ? _config.GuardianBonusCredits : 0);
            Integrity = Math.Min(MaxIntegrity, Integrity + Ring.RepairPerWave);
            WavesCleared++;
            _events.Add(new SimEvent(SimEventType.WaveCleared, Tick, value: WavesCleared, extra: interest));

            if (WavesCleared >= TotalWaves && !_config.Endless)
            {
                Phase = GamePhase.Victory;
                _events.Add(new SimEvent(SimEventType.Victory, Tick));
                return;
            }

            if (WavesCleared == TotalWaves && _config.Endless)
            {
                _events.Add(new SimEvent(SimEventType.Victory, Tick)); // the run is won; Endless continues for the leaderboard
            }

            OpenShop();
        }

        // ---------------------------------------------------------------- combat

        private void MoveEnemies()
        {
            foreach (Enemy enemy in _enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                enemy.AgeTicks++;
                long speed = enemy.SpeedPerTick;
                if (enemy.IsDashing)
                {
                    speed = speed * enemy.Definition.DashSpeedPermille / SimConstants.Permille;
                }

                if (Ring.SlowPermille > 0 && enemy.Radius <= Ring.SlowRadius)
                {
                    speed = speed * (SimConstants.Permille - Ring.SlowPermille) / SimConstants.Permille;
                }

                enemy.Radius -= speed;
                if (enemy.Radius > SimConstants.CoreRadius)
                {
                    continue;
                }

                enemy.ReachedCore = true;
                long damage = enemy.Definition.ContactDamage;
                Integrity -= damage;
                _events.Add(new SimEvent(SimEventType.CoreHit, Tick, enemy.Id, damage));
                if (Integrity <= 0 && Phase != GamePhase.Defeat)
                {
                    Integrity = 0;
                    Phase = GamePhase.Defeat;
                    DefeatedBy = enemy.Kind;
                    _events.Add(new SimEvent(SimEventType.Defeat, Tick));
                }
            }
        }

        private void FireModules()
        {
            for (int slot = 0; slot < Ring.SlotCount; slot++)
            {
                ModuleInstance module = Ring.At(slot);
                if (module == null || module.Category != ModuleCategory.Weapon)
                {
                    continue;
                }

                if (module.CooldownRemaining > 0)
                {
                    module.CooldownRemaining--;
                    continue;
                }

                Directions.PointAt(Directions.OfSlot(slot, Ring.SlotCount), SimConstants.RingRadius, out long mx, out long my);
                if (!Fire(module, mx, my))
                {
                    continue;
                }

                module.CooldownRemaining = module.EffectiveCooldown - 1;
                _events.Add(new SimEvent(SimEventType.ModuleFired, Tick, module.Id, extra: _targets.Count));
            }
        }

        /// <summary>Picks targets by behaviour and applies the hits (plus Echo). False when nothing is in range.</summary>
        private bool Fire(ModuleInstance module, long mx, long my)
        {
            ModuleDefinition definition = module.Definition;
            switch (definition.Behaviour)
            {
                case WeaponBehaviour.Chain:
                    CollectTargets(mx, my, module.EffectiveRange, 1);
                    if (_targets.Count == 0)
                    {
                        return false;
                    }

                    ChainFrom(_targets[0], definition);
                    break;
                case WeaponBehaviour.Pierce:
                    CollectTargets(mx, my, module.EffectiveRange, 1);
                    if (_targets.Count == 0)
                    {
                        return false;
                    }

                    CollectAlongLine(mx, my, _targets[0], module.EffectiveRange, definition.PierceWidthMilli * SimConstants.MilliToMicro);
                    break;
                case WeaponBehaviour.Splash:
                    Enemy centre = FarthestInRange(mx, my, module.EffectiveRange, definition.MinRangeMilli * SimConstants.MilliToMicro);
                    if (centre == null)
                    {
                        return false;
                    }

                    CollectAround(centre, definition.SplashRadiusMilli * SimConstants.MilliToMicro);
                    break;
                default:
                    CollectTargets(mx, my, module.EffectiveRange, definition.TargetCount);
                    if (_targets.Count == 0)
                    {
                        return false;
                    }

                    break;
            }

            // Chain damage falls off per jump; every other behaviour deals full damage to each target.
            for (int i = 0; i < _targets.Count; i++)
            {
                long damage = module.EffectiveDamage;
                if (definition.Behaviour == WeaponBehaviour.Chain)
                {
                    for (int jump = 0; jump < i; jump++)
                    {
                        damage = damage * definition.ChainFalloffPermille / SimConstants.Permille;
                    }
                }

                ApplyHit(_targets[i], damage, module.Id);
                if (module.EchoPermille > 0)
                {
                    ApplyHit(_targets[i], damage * module.EchoPermille / SimConstants.Permille, module.Id);
                }
            }

            return true;
        }

        /// <summary>Arc: from the first target, keeps jumping to the nearest unhit enemy within the jump range.</summary>
        private void ChainFrom(Enemy first, ModuleDefinition definition)
        {
            long jumpRange = definition.ChainRangeMilli * SimConstants.MilliToMicro;
            long jumpRangeSquared = jumpRange * jumpRange;
            Enemy current = first;
            while (_targets.Count < definition.ChainCount)
            {
                current.GetPosition(out long cx, out long cy);
                Enemy next = null;
                long best = long.MaxValue;
                foreach (Enemy enemy in _enemies)
                {
                    if (!enemy.IsAlive || _targets.Contains(enemy))
                    {
                        continue;
                    }

                    enemy.GetPosition(out long ex, out long ey);
                    long dx = ex - cx, dy = ey - cy;
                    long d = dx * dx + dy * dy;
                    if (d <= jumpRangeSquared && d < best)
                    {
                        best = d;
                        next = enemy;
                    }
                }

                if (next == null)
                {
                    break;
                }

                _targets.Add(next);
                current = next;
            }
        }

        /// <summary>Lance: every enemy within the range whose distance to the module→target line is under the half-width.</summary>
        private void CollectAlongLine(long mx, long my, Enemy target, long range, long halfWidth)
        {
            target.GetPosition(out long tx, out long ty);
            long dx = tx - mx, dy = ty - my;
            long length = IntegerSqrt(dx * dx + dy * dy);
            if (length == 0)
            {
                return;
            }

            long rangeSquared = range * range;
            foreach (Enemy enemy in _enemies)
            {
                if (!enemy.IsAlive || enemy == target)
                {
                    continue;
                }

                enemy.GetPosition(out long ex, out long ey);
                long px = ex - mx, py = ey - my;
                if (px * px + py * py > rangeSquared)
                {
                    continue;
                }

                // Ahead of the module (dot > 0) and close to the line (|cross| / length <= half-width).
                long dot = px * dx + py * dy;
                if (dot <= 0)
                {
                    continue;
                }

                long cross = px * dy - py * dx;
                if (Math.Abs(cross) / length <= halfWidth)
                {
                    _targets.Add(enemy);
                }
            }
        }

        /// <summary>Mortar: the farthest alive enemy in range that is at least the minimum distance away.</summary>
        private Enemy FarthestInRange(long mx, long my, long range, long minRange)
        {
            long rangeSquared = range * range;
            long minSquared = minRange * minRange;
            Enemy best = null;
            long bestDistance = -1;
            foreach (Enemy enemy in _enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                enemy.GetPosition(out long ex, out long ey);
                long dx = ex - mx, dy = ey - my;
                long d = dx * dx + dy * dy;
                if (d <= rangeSquared && d >= minSquared && d > bestDistance)
                {
                    bestDistance = d;
                    best = enemy;
                }
            }

            _targets.Clear();
            return best;
        }

        private void CollectAround(Enemy centre, long radius)
        {
            centre.GetPosition(out long cx, out long cy);
            long radiusSquared = radius * radius;
            _targets.Clear();
            foreach (Enemy enemy in _enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                enemy.GetPosition(out long ex, out long ey);
                long dx = ex - cx, dy = ey - cy;
                if (dx * dx + dy * dy <= radiusSquared)
                {
                    _targets.Add(enemy);
                }
            }
        }

        /// <summary>Integer square root (floor), deterministic on every platform.</summary>
        private static long IntegerSqrt(long value)
        {
            if (value <= 0)
            {
                return 0;
            }

            // Newton iteration on integers only (no floats anywhere in the simulation, D11).
            long x = value;
            long y = (x + 1) / 2;
            while (y < x)
            {
                x = y;
                y = (x + value / x) / 2;
            }

            return x;
        }

        /// <summary>Fills <see cref="_targets"/> with the enemies in range closest to the Core (ties: older first).</summary>
        private void CollectTargets(long x, long y, long range, int count)
        {
            _targets.Clear();
            long rangeSquared = range * range;
            foreach (Enemy enemy in _enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                enemy.GetPosition(out long ex, out long ey);
                long dx = ex - x;
                long dy = ey - y;
                if (dx * dx + dy * dy > rangeSquared)
                {
                    continue;
                }

                int insertAt = _targets.Count;
                while (insertAt > 0 && _targets[insertAt - 1].Radius > enemy.Radius)
                {
                    insertAt--;
                }

                if (insertAt < count)
                {
                    _targets.Insert(insertAt, enemy);
                    if (_targets.Count > count)
                    {
                        _targets.RemoveAt(_targets.Count - 1);
                    }
                }
            }
        }

        private void ApplyHit(Enemy enemy, long damage, int sourceModuleId)
        {
            if (!enemy.IsAlive)
            {
                return;
            }

            long shielded = damage * ShieldPermilleAt(enemy) / SimConstants.Permille;
            long dealt = Math.Max(SimConstants.HpScale, shielded - enemy.Armor);
            long applied = Math.Min(dealt, enemy.Hp);
            enemy.Hp -= dealt;
            TotalDamage += applied;
            _events.Add(new SimEvent(SimEventType.EnemyHit, Tick, enemy.Id, dealt, sourceModuleId));

            if (enemy.Hp <= 0)
            {
                enemy.Hp = 0;
                Kills++;
                _events.Add(new SimEvent(SimEventType.EnemyKilled, Tick, enemy.Id, dealt, sourceModuleId));
                if (enemy.Definition.SplitCount > 0 && _content.HasEnemy(enemy.Definition.SplitInto))
                {
                    // Splitter: the children appear where it died, slightly apart (GDD v0.2 §8).
                    int first = -(enemy.Definition.SplitCount - 1);
                    for (int i = 0; i < enemy.Definition.SplitCount; i++)
                    {
                        SpawnEnemy(enemy.Definition.SplitInto, enemy.Direction + first + i * 2, enemy.Radius, _pendingSpawns);
                    }
                }

                return;
            }

            if (enemy.Definition.SummonCount > 0)
            {
                // Every 25% of health lost, the Guardian calls a group of Swarmlets (GDD v0.2 §8).
                while (enemy.SummonsDone < 3 && enemy.Hp <= enemy.MaxHp * (3 - enemy.SummonsDone) / 4)
                {
                    enemy.SummonsDone++;
                    int count = enemy.Definition.SummonCount;
                    int first = -(count / 2);
                    for (int i = 0; i < count; i++)
                    {
                        SpawnEnemy(enemy.Definition.SummonKind, enemy.Direction + (first + i) * 3, enemy.Radius, _pendingSpawns);
                    }
                }
            }
        }

        /// <summary>Warden aura: the strongest shield of any other alive Warden within its radius (1000 = no shield).</summary>
        private long ShieldPermilleAt(Enemy target)
        {
            long best = SimConstants.Permille;
            target.GetPosition(out long tx, out long ty);
            foreach (Enemy warden in _enemies)
            {
                if (warden == target || !warden.IsAlive || warden.Definition.ShieldRadiusMilli <= 0)
                {
                    continue;
                }

                warden.GetPosition(out long wx, out long wy);
                long radius = warden.Definition.ShieldRadiusMilli * SimConstants.MilliToMicro;
                long dx = tx - wx, dy = ty - wy;
                if (dx * dx + dy * dy <= radius * radius && warden.Definition.ShieldPermille < best)
                {
                    best = warden.Definition.ShieldPermille;
                }
            }

            return best;
        }

        private void UsePulse()
        {
            foreach (Enemy enemy in _enemies)
            {
                if (!enemy.IsAlive || enemy.Radius > _config.PulseRadius)
                {
                    continue;
                }

                ApplyHit(enemy, _config.PulseDamage * (SimConstants.Permille + Ring.PulseDamageBonusPermille) / SimConstants.Permille, 0);
                if (enemy.IsAlive)
                {
                    enemy.Radius = Math.Min(SimConstants.SpawnRadius, enemy.Radius + _config.PulseKnockback);
                }
            }

            AddPendingSpawns();
            RemoveFinishedEnemies();
            PulseCooldownRemaining = _config.PulseCooldownTicks * (SimConstants.Permille - Ring.PulseCooldownReductionPermille) / SimConstants.Permille;
            _events.Add(new SimEvent(SimEventType.PulseUsed, Tick));
        }

        private void AddPendingSpawns()
        {
            if (_pendingSpawns.Count == 0)
            {
                return;
            }

            _enemies.AddRange(_pendingSpawns);
            _pendingSpawns.Clear();
        }

        private void RemoveFinishedEnemies() => _enemies.RemoveAll(e => !e.IsAlive);

        private void RecalculateRing()
        {
            long previousMax = MaxIntegrity;
            Ring.Recalculate(_config.CoreDamagePermille);
            MaxIntegrity = _config.BaseIntegrity + Ring.MaxIntegrityBonus;
            if (MaxIntegrity > previousMax)
            {
                Integrity += MaxIntegrity - previousMax;
            }

            Integrity = Math.Min(Integrity, MaxIntegrity);
        }

        // ---------------------------------------------------------------- undo (shop only, no randomness involved)

        private readonly struct ShopSnapshot
        {
            public readonly int Credits;
            public readonly int[] Offers;
            public readonly int SlotCount;
            public readonly ModuleSnapshot[] Slots;
            public readonly long Integrity;
            public readonly long MaxIntegrity;

            public ShopSnapshot(int credits, int[] offers, int slotCount, ModuleSnapshot[] slots, long integrity, long maxIntegrity)
            {
                Credits = credits;
                Offers = offers;
                SlotCount = slotCount;
                Slots = slots;
                Integrity = integrity;
                MaxIntegrity = maxIntegrity;
            }
        }

        private readonly struct ModuleSnapshot
        {
            public readonly int Id;
            public readonly ModuleKind Kind;
            public readonly int Level;
            public readonly int Invested;

            public ModuleSnapshot(ModuleInstance module)
            {
                Id = module.Id;
                Kind = module.Kind;
                Level = module.Level;
                Invested = module.Invested;
            }
        }

        private ShopSnapshot TakeSnapshot()
        {
            var slots = new ModuleSnapshot[Ring.SlotCount];
            for (int slot = 0; slot < Ring.SlotCount; slot++)
            {
                ModuleInstance module = Ring.At(slot);
                if (module != null)
                {
                    slots[slot] = new ModuleSnapshot(module);
                }
            }

            return new ShopSnapshot(Credits, (int[])_offers.Clone(), Ring.SlotCount, slots, Integrity, MaxIntegrity);
        }

        private void RestoreSnapshot(ShopSnapshot snapshot)
        {
            Credits = snapshot.Credits;
            Array.Copy(snapshot.Offers, _offers, _offers.Length);
            Ring.Reset(snapshot.SlotCount);
            for (int slot = 0; slot < Ring.SlotCount; slot++)
            {
                ModuleSnapshot saved = snapshot.Slots[slot];
                if (saved.Id == 0)
                {
                    continue;
                }

                var module = new ModuleInstance(saved.Id, _content.Module(saved.Kind), slot, saved.Invested) { Level = saved.Level };
                Ring.Place(module, slot);
            }

            Ring.Recalculate(_config.CoreDamagePermille);
            MaxIntegrity = snapshot.MaxIntegrity;
            Integrity = snapshot.Integrity;
        }

        // ---------------------------------------------------------------- previews (read-only, for the UI)

        /// <summary>Total damage per second of the ring (hundredths of HP), counting every target of every weapon.</summary>
        public long RingDps() => DpsOf(Ring);

        /// <summary>What the ring DPS would become if the offer were bought into the slot (or merged).</summary>
        public bool TryPreviewBuy(int offerIndex, int slot, out long dpsAfter, out bool merges)
        {
            dpsAfter = 0;
            merges = false;
            if (ValidateBuy(offerIndex, slot, out ModuleDefinition definition, out ModuleInstance mergeTarget) != CommandResult.Ok)
            {
                return false;
            }

            Ring copy = CopyRing();
            merges = mergeTarget != null;
            if (merges)
            {
                copy.At(mergeTarget.Slot).Level++;
            }
            else
            {
                copy.Place(new ModuleInstance(-1, definition, slot, definition.Cost), slot);
            }

            copy.Recalculate(_config.CoreDamagePermille);
            dpsAfter = DpsOf(copy);
            return true;
        }

        /// <summary>What the ring DPS would become after swapping two slots.</summary>
        public bool TryPreviewMove(int from, int to, out long dpsAfter)
        {
            dpsAfter = 0;
            if (Validate(Command.Move(from, to)) != CommandResult.Ok)
            {
                return false;
            }

            Ring copy = CopyRing();
            copy.Swap(from, to);
            copy.Recalculate(_config.CoreDamagePermille);
            dpsAfter = DpsOf(copy);
            return true;
        }

        /// <summary>What the ring DPS would become after opening an empty slot at <paramref name="insertAt"/>.</summary>
        public bool TryPreviewBuySlot(int insertAt, out long dpsAfter)
        {
            dpsAfter = 0;
            if (ValidateBuySlot(insertAt) != CommandResult.Ok)
            {
                return false;
            }

            Ring copy = CopyRing();
            copy.InsertSlot(insertAt);
            copy.Recalculate(_config.CoreDamagePermille);
            dpsAfter = DpsOf(copy);
            return true;
        }

        /// <summary>What the ring DPS would become after selling a slot.</summary>
        public bool TryPreviewSell(int slot, out long dpsAfter, out int refund)
        {
            dpsAfter = 0;
            refund = 0;
            ModuleInstance module = Ring.At(slot);
            if (module == null || Phase != GamePhase.Shop)
            {
                return false;
            }

            Ring copy = CopyRing();
            copy.Remove(slot);
            copy.Recalculate(_config.CoreDamagePermille);
            dpsAfter = DpsOf(copy);
            refund = ModuleRules.SellValue(module.Invested);
            return true;
        }

        private Ring CopyRing()
        {
            var copy = new Ring(Ring.SlotCount);
            for (int slot = 0; slot < Ring.SlotCount; slot++)
            {
                ModuleInstance module = Ring.At(slot);
                if (module != null)
                {
                    copy.Place(new ModuleInstance(module.Id, module.Definition, slot, module.Invested) { Level = module.Level }, slot);
                }
            }

            copy.Recalculate(_config.CoreDamagePermille);
            return copy;
        }

        private static long DpsOf(Ring ring)
        {
            long dps = 0;
            for (int slot = 0; slot < ring.SlotCount; slot++)
            {
                ModuleInstance module = ring.At(slot);
                if (module == null || module.Category != ModuleCategory.Weapon || module.EffectiveCooldown <= 0)
                {
                    continue;
                }

                long perShot = module.EffectiveDamage * module.Definition.ExpectedTargets;
                perShot += perShot * module.EchoPermille / SimConstants.Permille;
                dps += perShot * SimConstants.TicksPerSecond / module.EffectiveCooldown;
            }

            return dps;
        }
    }
}
