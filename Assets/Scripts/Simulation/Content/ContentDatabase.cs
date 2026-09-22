using System.Collections.Generic;

namespace TowerDefense.Simulation
{
    /// <summary>
    /// Plain balance data consumed by the simulation. In production it is filled from ScriptableObjects
    /// (Presentation/Content); the defaults below are the v0 values of GDD v0.2 and the reference for tests.
    /// </summary>
    public sealed class ContentDatabase
    {
        private const long Hp = SimConstants.HpScale;

        private readonly Dictionary<ModuleKind, ModuleDefinition> _modules = new Dictionary<ModuleKind, ModuleDefinition>();
        private readonly Dictionary<EnemyKind, EnemyDefinition> _enemies = new Dictionary<EnemyKind, EnemyDefinition>();
        private readonly List<ModuleKind> _shopPool = new List<ModuleKind>();
        private readonly List<ModuleKind> _moduleOrder = new List<ModuleKind>();
        private readonly List<EnemyKind> _enemyOrder = new List<EnemyKind>();

        public ModuleDefinition Module(ModuleKind kind) => _modules[kind];
        public EnemyDefinition Enemy(EnemyKind kind) => _enemies[kind];
        public bool HasEnemy(EnemyKind kind) => _enemies.ContainsKey(kind);

        /// <summary>Modules that can appear in the shop, in a fixed order (determinism).</summary>
        public IReadOnlyList<ModuleKind> ShopPool => _shopPool;

        /// <summary>Every module kind, in insertion order (stable, unlike dictionary order).</summary>
        public IReadOnlyList<ModuleKind> ModuleKinds => _moduleOrder;

        /// <summary>Every enemy kind, in insertion order.</summary>
        public IReadOnlyList<EnemyKind> EnemyKinds => _enemyOrder;

        public bool IsInShop(ModuleKind kind) => _shopPool.Contains(kind);

        public void Add(ModuleDefinition definition, bool inShop = true)
        {
            if (!_modules.ContainsKey(definition.Kind))
            {
                _moduleOrder.Add(definition.Kind);
            }

            _modules[definition.Kind] = definition;
            if (inShop && !_shopPool.Contains(definition.Kind))
            {
                _shopPool.Add(definition.Kind);
            }
        }

        public void Add(EnemyDefinition definition)
        {
            if (!_enemies.ContainsKey(definition.Kind))
            {
                _enemyOrder.Add(definition.Kind);
            }

            _enemies[definition.Kind] = definition;
        }

        /// <summary>The MVP roster (GDD v0.2 §6, §8): 14 modules, 6 enemies and the Guardian.</summary>
        public static ContentDatabase CreatePrototypeDefaults()
        {
            var db = new ContentDatabase();

            // Weapons
            db.Add(new ModuleDefinition(ModuleKind.Emitter, ModuleCategory.Weapon, Rarity.Common, 3)
            {
                Damage = 8 * Hp, CooldownTicks = 30, RangeMilli = 4000,
            });
            db.Add(new ModuleDefinition(ModuleKind.Scatter, ModuleCategory.Weapon, Rarity.Common, 4)
            {
                Damage = 5 * Hp, CooldownTicks = 45, RangeMilli = 3500, TargetCount = 3,
            });
            db.Add(new ModuleDefinition(ModuleKind.Arc, ModuleCategory.Weapon, Rarity.Uncommon, 5)
            {
                Damage = 6 * Hp, CooldownTicks = 40, RangeMilli = 4000, Behaviour = WeaponBehaviour.Chain,
                ChainCount = 4, ChainRangeMilli = 1500, ChainFalloffPermille = 900,
            });
            db.Add(new ModuleDefinition(ModuleKind.Lance, ModuleCategory.Weapon, Rarity.Uncommon, 5)
            {
                Damage = 18 * Hp, CooldownTicks = 90, RangeMilli = 6000, Behaviour = WeaponBehaviour.Pierce, PierceWidthMilli = 350,
            });
            db.Add(new ModuleDefinition(ModuleKind.Mortar, ModuleCategory.Weapon, Rarity.Rare, 7)
            {
                Damage = 16 * Hp, CooldownTicks = 90, RangeMilli = 7000, Behaviour = WeaponBehaviour.Splash,
                SplashRadiusMilli = 1200, MinRangeMilli = 2000,
            });

            // Boosters
            db.Add(new ModuleDefinition(ModuleKind.Amplifier, ModuleCategory.Booster, Rarity.Common, 3)
            {
                DamageMultiplierBonusPermille = 500,
            });
            db.Add(new ModuleDefinition(ModuleKind.Lens, ModuleCategory.Booster, Rarity.Common, 3)
            {
                RangeBonusMilli = 1500, FlatDamageBonus = 2 * Hp,
            });
            db.Add(new ModuleDefinition(ModuleKind.Overclock, ModuleCategory.Booster, Rarity.Uncommon, 4)
            {
                CooldownReductionPermille = 250,
            });
            db.Add(new ModuleDefinition(ModuleKind.Echo, ModuleCategory.Booster, Rarity.Rare, 6)
            {
                EchoPermille = 500,
            });

            // Economy and utility
            db.Add(new ModuleDefinition(ModuleKind.Bank, ModuleCategory.Economy, Rarity.Uncommon, 4)
            {
                InterestCapBonus = 1, CreditsPerWave = 1,
            });
            db.Add(new ModuleDefinition(ModuleKind.Salvage, ModuleCategory.Economy, Rarity.Common, 3)
            {
                KillsPerCredit = 10,
            });
            db.Add(new ModuleDefinition(ModuleKind.Bulwark, ModuleCategory.Economy, Rarity.Common, 3)
            {
                MaxIntegrityBonus = 25 * Hp, RepairPerWave = 5 * Hp,
            });
            db.Add(new ModuleDefinition(ModuleKind.Frost, ModuleCategory.Economy, Rarity.Uncommon, 4)
            {
                SlowPermille = 250, SlowRadiusMilli = 3000,
            });
            db.Add(new ModuleDefinition(ModuleKind.Capacitor, ModuleCategory.Economy, Rarity.Uncommon, 4)
            {
                PulseCooldownReductionPermille = 200, PulseDamageBonusPermille = 500,
            });

            // Enemies
            db.Add(new EnemyDefinition(EnemyKind.Drifter, 20 * Hp, 0, 900, 5 * Hp, 1000));
            db.Add(new EnemyDefinition(EnemyKind.Swarmlet, 6 * Hp, 0, 1300, 2 * Hp, 300, groupSize: 5));
            db.Add(new EnemyDefinition(EnemyKind.Brute, 70 * Hp, 3 * Hp, 550, 15 * Hp, 3000));
            db.Add(new EnemyDefinition(EnemyKind.Dasher, 14 * Hp, 0, 800, 5 * Hp, 1500)
            {
                DashEveryTicks = 3 * SimConstants.TicksPerSecond, DashTicks = SimConstants.TicksPerSecond / 2, DashSpeedPermille = 3000,
            });
            db.Add(new EnemyDefinition(EnemyKind.Splitter, 30 * Hp, 0, 850, 6 * Hp, 2000)
            {
                SplitInto = EnemyKind.Swarmlet, SplitCount = 2,
            });
            db.Add(new EnemyDefinition(EnemyKind.Warden, 40 * Hp, 1 * Hp, 700, 8 * Hp, 3000)
            {
                ShieldRadiusMilli = 1500, ShieldPermille = 500,
            });
            db.Add(new EnemyDefinition(EnemyKind.Guardian, 400 * Hp, 5 * Hp, 400, 40 * Hp, 0)
            {
                SummonCount = 5, SummonKind = EnemyKind.Swarmlet,
            });

            return db;
        }
    }
}
