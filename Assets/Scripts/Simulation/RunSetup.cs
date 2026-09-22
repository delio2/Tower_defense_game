namespace TowerDefense.Simulation
{
    /// <summary>The Core variant chosen at run start (GDD v0.2 §4): changes the starting rules, never the pool.</summary>
    public enum CoreType : byte
    {
        Standard = 0,
        Merchant = 1,
        Bastion = 2,
        Glass = 3,
    }

    /// <summary>How the run is played; part of the replay so leaderboards stay comparable (GDD v0.2 §12–13).</summary>
    public enum RunMode : byte
    {
        Run = 0,
        Endless = 1,
        Daily = 2,
        Weekly = 3,
        Practice = 4,
    }

    /// <summary>
    /// Applies the run-defining choices (Core type, Grade, mode) to a <see cref="RunConfig"/>. The same function runs
    /// on the client and in replay verification, so a replay only needs to store the choices, not the numbers.
    /// </summary>
    public static class RunSetup
    {
        public const int MaxGrade = 10;

        public static RunConfig Create(ulong seed, CoreType core, int grade = 0, RunMode mode = RunMode.Run)
        {
            var config = new RunConfig { Seed = seed };
            Configure(config, core, grade, mode);
            return config;
        }

        public static void Configure(RunConfig config, CoreType core, int grade, RunMode mode)
        {
            config.Core = core;
            config.Grade = grade;
            config.Mode = mode;
            config.Endless = mode == RunMode.Endless;

            switch (core)
            {
                case CoreType.Merchant:
                    config.InterestCap += 1;
                    config.StartingSlots = 5;
                    config.StartingCredits = 10;
                    config.StartingModules = new ModuleKind[0];
                    break;
                case CoreType.Bastion:
                    config.BaseIntegrity = 150 * SimConstants.HpScale;
                    config.PulseCooldownTicks = config.PulseCooldownTicks * 750 / SimConstants.Permille;
                    config.StartingModules = new[] { ModuleKind.Bulwark };
                    config.StartingCredits = 4;
                    break;
                case CoreType.Glass:
                    config.CoreDamagePermille = 1500;
                    config.BaseIntegrity = 50 * SimConstants.HpScale;
                    config.StartingModules = new[] { ModuleKind.Emitter, ModuleKind.Amplifier };
                    config.StartingCredits = 2;
                    break;
            }

            // Grades (Ascension-like, `07` §2.2 proposal): one readable rule each, cumulative.
            if (grade >= 1)
            {
                config.EnemyHpBonusPermille += 100;
            }

            if (grade >= 2)
            {
                config.CreditsPerWave -= 1;
            }

            if (grade >= 3)
            {
                config.EliteFromAct = 1;
            }
        }
    }
}
