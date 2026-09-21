namespace TowerDefense.Simulation
{
    public enum ModuleKind : byte
    {
        Emitter = 0,
        Scatter = 1,
        Arc = 2,
        Lance = 3,
        Mortar = 4,
        Amplifier = 10,
        Lens = 11,
        Overclock = 12,
        Echo = 13,
        Bank = 20,
        Salvage = 21,
        Bulwark = 22,
        Frost = 23,
        Capacitor = 24,
    }

    public enum ModuleCategory : byte
    {
        Weapon = 0,
        Booster = 1,
        Economy = 2,
    }

    public enum Rarity : byte
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
    }

    /// <summary>
    /// Immutable balance data for one module (GDD v0.2 §6). Effects of boosters and economy modules are expressed
    /// at level 1 and scaled by <see cref="ModuleRules.EffectScale"/>.
    /// </summary>
    public sealed class ModuleDefinition
    {
        public ModuleKind Kind { get; }
        public ModuleCategory Category { get; }
        public Rarity Rarity { get; }
        public int Cost { get; }

        // Weapons
        /// <summary>Damage per hit in hundredths of HP.</summary>
        public long Damage { get; set; }
        public int CooldownTicks { get; set; }
        public int RangeMilli { get; set; }
        public int TargetCount { get; set; } = 1;

        // Boosters (applied to both neighbours)
        public int DamageMultiplierBonusPermille { get; set; }
        public long FlatDamageBonus { get; set; }
        public int RangeBonusMilli { get; set; }
        public int CooldownReductionPermille { get; set; }

        // Economy and utility
        public int InterestCapBonus { get; set; }
        public int CreditsPerWave { get; set; }
        public long MaxIntegrityBonus { get; set; }
        public long RepairPerWave { get; set; }

        public ModuleDefinition(ModuleKind kind, ModuleCategory category, Rarity rarity, int cost)
        {
            Kind = kind;
            Category = category;
            Rarity = rarity;
            Cost = cost;
        }
    }

    public static class ModuleRules
    {
        public const int MaxLevel = 3;

        /// <summary>Weapon damage per level, permille (x1.0, x1.8, x3.0).</summary>
        private static readonly int[] WeaponScale = { 0, 1000, 1800, 3000 };

        /// <summary>Booster and economy effect per level, permille (x1.0, x1.6, x2.2).</summary>
        private static readonly int[] EffectScaleTable = { 0, 1000, 1600, 2200 };

        public static int WeaponDamageScale(int level) => WeaponScale[level];

        public static int EffectScale(int level) => EffectScaleTable[level];

        /// <summary>Scales a level-1 effect value, rounding half up.</summary>
        public static long ScaleEffect(long value, int level) => (value * EffectScale(level) + SimConstants.Permille / 2) / SimConstants.Permille;

        /// <summary>Sell refund: half of the credits invested, rounded down, at least 1.</summary>
        public static int SellValue(int invested) => invested / 2 < 1 ? 1 : invested / 2;
    }
}
