using System;

namespace TowerDefense.Simulation
{
    /// <summary>
    /// The ring of slots around the Core. Boosters affect their two neighbours; the ring is circular (D18).
    /// </summary>
    public sealed class Ring
    {
        public const int MaxSlots = 8;
        public const int MinCooldownTicks = 5;

        private readonly ModuleInstance[] _slots = new ModuleInstance[MaxSlots];

        public int SlotCount { get; internal set; }

        // Aggregated economy effects (recomputed with the ring).
        public int InterestCapBonus { get; private set; }
        public int CreditsPerWave { get; private set; }
        public long MaxIntegrityBonus { get; private set; }
        public long RepairPerWave { get; private set; }

        /// <summary>Salvage: Credits per kill in permille (100 = one Credit every 10 kills).</summary>
        public int SalvagePermillePerKill { get; private set; }

        /// <summary>Frost: strongest slow on the ring and its radius from the Core.</summary>
        public int SlowPermille { get; private set; }
        public long SlowRadius { get; private set; }

        /// <summary>Capacitor: Pulse cooldown reduction (capped at 90%) and damage bonus, permille.</summary>
        public int PulseCooldownReductionPermille { get; private set; }
        public int PulseDamageBonusPermille { get; private set; }

        public Ring(int slotCount)
        {
            SlotCount = slotCount;
        }

        public ModuleInstance At(int slot) => IsValidSlot(slot) ? _slots[slot] : null;

        public bool IsValidSlot(int slot) => slot >= 0 && slot < SlotCount;

        public int LeftOf(int slot) => (slot - 1 + SlotCount) % SlotCount;

        public int RightOf(int slot) => (slot + 1) % SlotCount;

        public bool HasFreeSlot()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (_slots[i] == null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The lowest-slot module of this kind that can still level up, or null.</summary>
        public ModuleInstance FindMergeTarget(ModuleKind kind)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                ModuleInstance module = _slots[i];
                if (module != null && module.Kind == kind && module.Level < ModuleRules.MaxLevel)
                {
                    return module;
                }
            }

            return null;
        }

        internal void Place(ModuleInstance module, int slot)
        {
            _slots[slot] = module;
            module.Slot = slot;
        }

        internal void Remove(int slot) => _slots[slot] = null;

        /// <summary>Opens an empty slot at <paramref name="index"/> (0..SlotCount); later modules shift by one.</summary>
        internal void InsertSlot(int index)
        {
            for (int slot = SlotCount - 1; slot >= index; slot--)
            {
                _slots[slot + 1] = _slots[slot];
                if (_slots[slot + 1] != null)
                {
                    _slots[slot + 1].Slot = slot + 1;
                }
            }

            _slots[index] = null;
            SlotCount++;
        }

        /// <summary>Empties every slot and sets the slot count (undo restore).</summary>
        internal void Reset(int slotCount)
        {
            for (int slot = 0; slot < MaxSlots; slot++)
            {
                _slots[slot] = null;
            }

            SlotCount = slotCount;
        }

        internal void Swap(int from, int to)
        {
            ModuleInstance a = _slots[from];
            ModuleInstance b = _slots[to];
            _slots[from] = b;
            _slots[to] = a;
            if (a != null)
            {
                a.Slot = to;
            }

            if (b != null)
            {
                b.Slot = from;
            }
        }

        /// <summary>
        /// Recomputes weapon stats from levels and neighbour boosters (GDD v0.2 §7):
        /// damage = (base x level + flat bonuses) x product of multipliers x global multiplier.
        /// </summary>
        public void Recalculate(int globalDamagePermille)
        {
            InterestCapBonus = 0;
            CreditsPerWave = 0;
            MaxIntegrityBonus = 0;
            RepairPerWave = 0;
            SalvagePermillePerKill = 0;
            SlowPermille = 0;
            SlowRadius = 0;
            PulseCooldownReductionPermille = 0;
            PulseDamageBonusPermille = 0;

            for (int slot = 0; slot < SlotCount; slot++)
            {
                ModuleInstance module = _slots[slot];
                if (module == null)
                {
                    continue;
                }

                ModuleDefinition definition = module.Definition;
                if (definition.Category == ModuleCategory.Economy)
                {
                    InterestCapBonus += (int)ModuleRules.ScaleEffect(definition.InterestCapBonus, module.Level);
                    CreditsPerWave += (int)ModuleRules.ScaleEffect(definition.CreditsPerWave, module.Level);
                    MaxIntegrityBonus += ModuleRules.ScaleEffect(definition.MaxIntegrityBonus, module.Level);
                    RepairPerWave += ModuleRules.ScaleEffect(definition.RepairPerWave, module.Level);
                    if (definition.KillsPerCredit > 0)
                    {
                        SalvagePermillePerKill += (int)ModuleRules.ScaleEffect(SimConstants.Permille / definition.KillsPerCredit, module.Level);
                    }

                    if (definition.SlowPermille > 0)
                    {
                        int slow = (int)Math.Min(900, ModuleRules.ScaleEffect(definition.SlowPermille, module.Level));
                        if (slow > SlowPermille)
                        {
                            SlowPermille = slow;
                            SlowRadius = definition.SlowRadiusMilli * SimConstants.MilliToMicro;
                        }
                    }

                    PulseCooldownReductionPermille = (int)Math.Min(900, PulseCooldownReductionPermille + ModuleRules.ScaleEffect(definition.PulseCooldownReductionPermille, module.Level));
                    PulseDamageBonusPermille += (int)ModuleRules.ScaleEffect(definition.PulseDamageBonusPermille, module.Level);
                }

                if (definition.Category != ModuleCategory.Weapon)
                {
                    module.EffectiveDamage = 0;
                    module.DamageMultiplierPermille = SimConstants.Permille;
                    module.EchoPermille = 0;
                    continue;
                }

                long flat = 0;
                long multiplier = SimConstants.Permille;
                long rangeMilli = definition.RangeMilli;
                long cooldown = definition.CooldownTicks;
                long echo = 0;

                int left = LeftOf(slot);
                int right = RightOf(slot);
                ApplyBooster(_slots[left], ref flat, ref multiplier, ref rangeMilli, ref cooldown, ref echo);
                if (right != left)
                {
                    ApplyBooster(_slots[right], ref flat, ref multiplier, ref rangeMilli, ref cooldown, ref echo);
                }

                long baseDamage = definition.Damage * ModuleRules.WeaponDamageScale(module.Level) / SimConstants.Permille;
                long damage = (baseDamage + flat) * multiplier / SimConstants.Permille;
                damage = damage * globalDamagePermille / SimConstants.Permille;

                module.EffectiveDamage = Math.Max(1, damage);
                module.DamageMultiplierPermille = multiplier * globalDamagePermille / SimConstants.Permille;
                module.EffectiveRange = rangeMilli * SimConstants.MilliToMicro;
                module.EffectiveCooldown = (int)Math.Max(MinCooldownTicks, cooldown);
                module.EchoPermille = (int)Math.Min(SimConstants.Permille, echo);
            }
        }

        private static void ApplyBooster(ModuleInstance neighbour, ref long flat, ref long multiplier, ref long rangeMilli, ref long cooldown, ref long echo)
        {
            if (neighbour == null || neighbour.Category != ModuleCategory.Booster)
            {
                return;
            }

            ModuleDefinition booster = neighbour.Definition;
            int level = neighbour.Level;
            flat += ModuleRules.ScaleEffect(booster.FlatDamageBonus, level);
            multiplier = multiplier * (SimConstants.Permille + ModuleRules.ScaleEffect(booster.DamageMultiplierBonusPermille, level)) / SimConstants.Permille;
            rangeMilli += ModuleRules.ScaleEffect(booster.RangeBonusMilli, level);
            long reduction = Math.Min(900, ModuleRules.ScaleEffect(booster.CooldownReductionPermille, level));
            cooldown = cooldown * (SimConstants.Permille - reduction) / SimConstants.Permille;
            echo += ModuleRules.ScaleEffect(booster.EchoPermille, level);
        }
    }
}
