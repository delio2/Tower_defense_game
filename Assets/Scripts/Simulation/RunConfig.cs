namespace TowerDefense.Simulation
{
    /// <summary>
    /// Parameters of a single run. Balance values follow GDD v0.2 (v0).
    /// </summary>
    public sealed class RunConfig
    {
        /// <summary>Bumped on every balance change: replays only verify against the same version (D19).</summary>
        public const string BalanceVersion = "0.3.0";

        /// <summary>Game (client) version recorded in replays; mismatches are informative, not blocking.</summary>
        public const string GameVersion = "0.2.0";

        public ulong Seed = 1;

        // Run-defining choices (set through RunSetup so replays can rebuild the config from them)
        public CoreType Core = CoreType.Standard;
        public int Grade;
        public RunMode Mode = RunMode.Run;

        /// <summary>After the last act the run continues with growing waves instead of ending in victory.</summary>
        public bool Endless;

        /// <summary>3 acts of 5 waves + Guardian (GDD v0.2 §2). Tests and probes may shorten it.</summary>
        public int Acts = 3;
        public int WavesPerAct = 6;

        public int StartingCredits = 6;
        public long BaseIntegrity = 100 * SimConstants.HpScale;
        public int StartingSlots = 6;
        public ModuleKind[] StartingModules = { ModuleKind.Emitter };

        /// <summary>Global damage multiplier of the Core type, permille (Glass core: 1500).</summary>
        public int CoreDamagePermille = SimConstants.Permille;

        // Wave growth (GDD v0.2 §9)
        public int HpGrowthPermille = 1120;
        public int BudgetGrowthPermille = 1100;
        public int FirstWaveBudgetMilli = 8000;
        public int SpawnWindowTicks = 20 * SimConstants.TicksPerSecond;

        /// <summary>Flat enemy HP bonus (Grade 1: +10%).</summary>
        public int EnemyHpBonusPermille;

        /// <summary>First act with elites (2 by default, 1 at Grade 3).</summary>
        public int EliteFromAct = 2;

        // Economy (GDD v0.2 §10)
        public int CreditsPerWave = 4;
        public int GuardianBonusCredits = 3;
        public int InterestStep = 5;
        public int InterestCap = 5;
        public int FirstRerollCost = 1;
        public int ShopOffers = 4;

        /// <summary>Extra ring slot: offered after the first Guardian, up to <see cref="Ring.MaxSlots"/> (GDD v0.2 §5, §10).</summary>
        public int ExtraSlotCost = 8;

        // Pulse (GDD v0.2 §4)
        public long PulseDamage = 20 * SimConstants.HpScale;
        public long PulseRadius = 3000 * SimConstants.MilliToMicro;
        public long PulseKnockback = 1500 * SimConstants.MilliToMicro;
        public int PulseCooldownTicks = 20 * SimConstants.TicksPerSecond;

        public int TotalWaves => Acts * WavesPerAct;
    }
}
