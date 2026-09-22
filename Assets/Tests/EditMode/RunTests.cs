using NUnit.Framework;

namespace TowerDefense.Simulation.Tests
{
    /// <summary>Core types, Grades, Endless and the R2 replay format (GDD v0.2 §4, §9, §12, §13).</summary>
    public class RunTests
    {
        private const long Hp = SimConstants.HpScale;

        private static GameSimulation Start(CoreType core, int grade = 0, RunMode mode = RunMode.Run, ulong seed = 4)
        {
            return new GameSimulation(RunSetup.Create(seed, core, grade, mode), ContentDatabase.CreatePrototypeDefaults());
        }

        [Test]
        public void Merchant_StartsRichWithFiveSlotsAndHigherInterestCap()
        {
            GameSimulation sim = Start(CoreType.Merchant);
            Assert.AreEqual(10, sim.Credits);
            Assert.AreEqual(5, sim.Ring.SlotCount);
            Assert.IsNull(sim.Ring.At(0));
            Assert.AreEqual(6, sim.Config.InterestCap);
        }

        [Test]
        public void Bastion_HasMoreIntegrityABulwarkAndAFasterPulse()
        {
            GameSimulation sim = Start(CoreType.Bastion);
            Assert.AreEqual(ModuleKind.Bulwark, sim.Ring.At(0).Kind);
            Assert.AreEqual((150 + 25) * Hp, sim.MaxIntegrity, "150 base + Bulwark");
            Assert.AreEqual(900, sim.Config.PulseCooldownTicks, "-25%");
            Assert.AreEqual(4, sim.Credits);
        }

        [Test]
        public void Glass_MultipliesDamageAndHalvesIntegrity()
        {
            GameSimulation sim = Start(CoreType.Glass);
            Assert.AreEqual(50 * Hp, sim.MaxIntegrity);
            Assert.AreEqual(ModuleKind.Emitter, sim.Ring.At(0).Kind);
            Assert.AreEqual(ModuleKind.Amplifier, sim.Ring.At(1).Kind);
            Assert.AreEqual(8 * Hp * 15 / 10 * 15 / 10, sim.Ring.At(0).EffectiveDamage, "8 x 1.5 (Amplifier) x 1.5 (Glass)");
            Assert.AreEqual(2, sim.Credits);
        }

        [Test]
        public void Grades_AddOneRuleEach()
        {
            RunConfig grade1 = RunSetup.Create(1, CoreType.Standard, 1);
            var director = new WaveDirector(grade1, ContentDatabase.CreatePrototypeDefaults(), Pcg32.ForStream(1, RngStream.Waves));
            Assert.AreEqual(22 * Hp, director.EnemyMaxHp(ContentDatabase.CreatePrototypeDefaults().Enemy(EnemyKind.Drifter), 1), "+10% HP");
            Assert.AreEqual(4, grade1.CreditsPerWave);
            Assert.AreEqual(2, grade1.EliteFromAct);

            RunConfig grade3 = RunSetup.Create(1, CoreType.Standard, 3);
            Assert.AreEqual(3, grade3.CreditsPerWave, "Grade 2: -1 Credit per wave");
            Assert.AreEqual(1, grade3.EliteFromAct, "Grade 3: elites from act 1");
            var eliteDirector = new WaveDirector(grade3, ContentDatabase.CreatePrototypeDefaults(), Pcg32.ForStream(1, RngStream.Waves));
            bool anyElite = false;
            for (int wave = 1; wave <= 6; wave++)
            {
                foreach (SpawnEntry entry in eliteDirector.BuildWave(wave))
                {
                    anyElite |= entry.IsElite;
                }
            }

            Assert.IsTrue(anyElite);
        }

        [Test]
        public void Endless_ContinuesAfterTheLastActUntilDefeat()
        {
            var config = RunSetup.Create(6, CoreType.Standard, 0, RunMode.Endless);
            config.Acts = 1;
            config.StartingCredits = 60;
            RunResult result = BalanceBot.Play(config, ContentDatabase.CreatePrototypeDefaults(), BotStrategy.MaxDps);
            Assert.IsTrue(result.Won, "the act was cleared");
            Assert.Greater(result.WavesCleared, 6, "play went on past the act");
            Assert.IsTrue(result.DefeatedBy.HasValue, "Endless ends only in defeat");
        }

        [Test]
        public void ReplayR2_RoundTripsChoicesAndVerifies()
        {
            var config = RunSetup.Create(12, CoreType.Glass, 2, RunMode.Run);
            var sim = new GameSimulation(config, ContentDatabase.CreatePrototypeDefaults());
            BalanceBot.Play(sim, BotStrategy.Naive);

            string text = Replay.Record(sim).Serialize();
            StringAssert.StartsWith("R2|", text);
            Replay parsed = Replay.Deserialize(text);
            Assert.AreEqual(CoreType.Glass, parsed.Core);
            Assert.AreEqual(2, parsed.Grade);
            Assert.AreEqual(RunMode.Run, parsed.Mode);
            Assert.AreEqual(RunConfig.GameVersion, parsed.GameVersion);

            ReplayCheck check = ReplayVerifier.Verify(parsed, ContentDatabase.CreatePrototypeDefaults(), () => new RunConfig());
            Assert.IsTrue(check.IsValid, check.Reason);
        }

        [Test]
        public void ReplayR1_IsStillReadable()
        {
            Replay legacy = Replay.Deserialize("R1|0.3.0|5|123|2|4500|900|0,4,0,0;60,5,0,0");
            Assert.AreEqual(5UL, legacy.Seed);
            Assert.AreEqual(CoreType.Standard, legacy.Core);
            Assert.AreEqual(0, legacy.Grade);
            Assert.AreEqual(2, legacy.Entries.Count);
            Assert.AreEqual(CommandType.Pulse, legacy.Entries[1].Command.Type);
        }
    }
}
