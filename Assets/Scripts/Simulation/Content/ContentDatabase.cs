using System.Collections.Generic;

namespace TowerDefense.Simulation
{
    /// <summary>
    /// Plain balance data consumed by the simulation. In production it is filled from ScriptableObjects;
    /// the prototype uses the v0 values of GDD v0.2 directly.
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

        /// <summary>Prototype roster (GDD v0.2 §19): 7 modules, Drifter/Swarmlet/Brute and the Guardian.</summary>
        public static ContentDatabase CreatePrototypeDefaults()
        {
            var db = new ContentDatabase();

            db.Add(new ModuleDefinition(ModuleKind.Emitter, ModuleCategory.Weapon, Rarity.Common, 3)
            {
                Damage = 8 * Hp, CooldownTicks = 30, RangeMilli = 4000,
            });
            db.Add(new ModuleDefinition(ModuleKind.Scatter, ModuleCategory.Weapon, Rarity.Common, 4)
            {
                Damage = 5 * Hp, CooldownTicks = 45, RangeMilli = 3500, TargetCount = 3,
            });
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
            db.Add(new ModuleDefinition(ModuleKind.Bank, ModuleCategory.Economy, Rarity.Uncommon, 4)
            {
                InterestCapBonus = 1, CreditsPerWave = 1,
            });
            db.Add(new ModuleDefinition(ModuleKind.Bulwark, ModuleCategory.Economy, Rarity.Common, 3)
            {
                MaxIntegrityBonus = 25 * Hp, RepairPerWave = 5 * Hp,
            });

            db.Add(new EnemyDefinition(EnemyKind.Drifter, 20 * Hp, 0, 900, 5 * Hp, 1000));
            db.Add(new EnemyDefinition(EnemyKind.Swarmlet, 6 * Hp, 0, 1300, 2 * Hp, 300, groupSize: 5));
            db.Add(new EnemyDefinition(EnemyKind.Brute, 70 * Hp, 3 * Hp, 550, 15 * Hp, 3000));
            db.Add(new EnemyDefinition(EnemyKind.Guardian, 400 * Hp, 5 * Hp, 400, 40 * Hp, 0));

            return db;
        }
    }
}
