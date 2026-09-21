using System.Collections.Generic;
using NUnit.Framework;

namespace TowerDefense.Simulation.Tests
{
    public class SimulationTests
    {
        private const long Hp = SimConstants.HpScale;

        private static RunConfig Config(ulong seed = 42, int credits = 6)
        {
            return new RunConfig { Seed = seed, StartingCredits = credits };
        }

        private static GameSimulation Create(ulong seed = 42, int credits = 6)
        {
            return new GameSimulation(Config(seed, credits), ContentDatabase.CreatePrototypeDefaults());
        }

        private static int FindOffer(GameSimulation sim, ModuleKind kind)
        {
            for (int i = 0; i < sim.OfferCount; i++)
            {
                if (sim.OfferAt(i) == kind)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Rerolls (with plenty of credits) until the shop offers <paramref name="kind"/>.</summary>
        private static int RerollUntil(GameSimulation sim, ModuleKind kind)
        {
            for (int attempt = 0; attempt < 200; attempt++)
            {
                int index = FindOffer(sim, kind);
                if (index >= 0)
                {
                    return index;
                }

                sim.Enqueue(Command.Reroll());
                sim.ApplyPendingCommandsNow();
            }

            Assert.Fail($"{kind} never offered");
            return -1;
        }

        [Test]
        public void Directions_TableIsUnitLength()
        {
            for (int d = 0; d < Directions.Count; d++)
            {
                long lengthSquared = (long)Directions.Cos(d) * Directions.Cos(d) + (long)Directions.Sin(d) * Directions.Sin(d);
                Assert.AreEqual(100_000_000, lengthSquared, 20_000, $"direction {d}");
            }

            Assert.AreEqual(Directions.Up, Directions.OfSlot(0, 6));
            Assert.AreEqual(32, Directions.Normalize(Directions.OfSlot(0, 6) - Directions.OfSlot(1, 6)));
        }

        [Test]
        public void NewRun_StartsInShopWithEmitter()
        {
            GameSimulation sim = Create();
            Assert.AreEqual(GamePhase.Shop, sim.Phase);
            Assert.AreEqual(6, sim.Credits);
            Assert.AreEqual(ModuleKind.Emitter, sim.Ring.At(0).Kind);
            Assert.AreEqual(8 * Hp, sim.Ring.At(0).EffectiveDamage);
            Assert.IsNotNull(sim.OfferAt(0));
            Assert.Greater(sim.NextWavePreview.Count, 0);
        }

        [Test]
        public void Amplifier_MultipliesOnlyItsNeighbours()
        {
            GameSimulation sim = Create(credits: 500);
            int amp = RerollUntil(sim, ModuleKind.Amplifier);
            sim.Enqueue(Command.Buy(amp, 1));
            sim.ApplyPendingCommandsNow();

            Assert.AreEqual(12 * Hp, sim.Ring.At(0).EffectiveDamage, "Emitter next to Amplifier: 8 x 1.5");

            // A second Emitter away from the Amplifier is unaffected. Buying one merges instead, so move first.
            sim.Enqueue(Command.Move(0, 3));
            sim.ApplyPendingCommandsNow();
            Assert.AreEqual(8 * Hp, sim.Ring.At(3).EffectiveDamage, "Emitter no longer adjacent");
        }

        [Test]
        public void TwoAmplifiers_Stack_Multiplicatively()
        {
            GameSimulation sim = Create(credits: 500);
            sim.Enqueue(Command.Buy(RerollUntil(sim, ModuleKind.Amplifier), 1));
            sim.ApplyPendingCommandsNow();

            // The second Amplifier would merge into the first; instead we check the level-2 value (x1.8).
            sim.Enqueue(Command.Buy(RerollUntil(sim, ModuleKind.Amplifier), 5));
            sim.ApplyPendingCommandsNow();
            Assert.AreEqual(2, sim.Ring.At(1).Level, "duplicate merged");
            Assert.AreEqual(8 * Hp * 1800 / 1000, sim.Ring.At(0).EffectiveDamage);
        }

        [Test]
        public void Merge_LevelsUpAndAccumulatesInvestment()
        {
            GameSimulation sim = Create(credits: 500);
            sim.Enqueue(Command.Buy(RerollUntil(sim, ModuleKind.Emitter), 2));
            sim.ApplyPendingCommandsNow();

            ModuleInstance emitter = sim.Ring.At(0);
            Assert.AreEqual(2, emitter.Level);
            Assert.AreEqual(6, emitter.Invested);
            Assert.IsNull(sim.Ring.At(2), "merge does not use the chosen slot");
            Assert.AreEqual(8 * Hp * 1800 / 1000, emitter.EffectiveDamage);
        }

        [Test]
        public void Lens_AddsRangeAndFlatDamage()
        {
            GameSimulation sim = Create(credits: 500);
            sim.Enqueue(Command.Buy(RerollUntil(sim, ModuleKind.Lens), 5));
            sim.ApplyPendingCommandsNow();

            ModuleInstance emitter = sim.Ring.At(0);
            Assert.AreEqual(10 * Hp, emitter.EffectiveDamage);
            Assert.AreEqual(5500 * SimConstants.MilliToMicro, emitter.EffectiveRange);
        }

        [Test]
        public void Sell_RefundsHalf_AndRerollCostGrows()
        {
            GameSimulation sim = Create(credits: 20);
            sim.Enqueue(Command.Reroll());
            sim.Enqueue(Command.Reroll());
            sim.ApplyPendingCommandsNow();
            Assert.AreEqual(20 - 1 - 2, sim.Credits);
            Assert.AreEqual(3, sim.RerollCost);

            sim.Enqueue(Command.Sell(0));
            sim.ApplyPendingCommandsNow();
            Assert.AreEqual(17 + 1, sim.Credits, "Emitter cost 3, refund 1");
            Assert.IsNull(sim.Ring.At(0));
        }

        [Test]
        public void Buy_RejectsOccupiedSlotAndMissingCredits()
        {
            GameSimulation sim = Create(credits: 2);
            Assert.AreEqual(CommandResult.NotEnoughCredits, sim.Validate(Command.Buy(0, 1)), "every module costs 3+");

            GameSimulation rich = Create(credits: 50);
            int nonEmitter = 0;
            while (rich.OfferAt(nonEmitter) == ModuleKind.Emitter)
            {
                nonEmitter++;
            }

            Assert.AreEqual(CommandResult.SlotOccupied, rich.Validate(Command.Buy(nonEmitter, 0)));
        }

        [Test]
        public void UndefendedCore_LosesIntegrity()
        {
            GameSimulation sim = Create();
            sim.Enqueue(Command.Sell(0));
            sim.Enqueue(Command.StartWave());
            for (int i = 0; i < 40 * SimConstants.TicksPerSecond && !sim.IsOver; i++)
            {
                sim.Step();
            }

            Assert.Less(sim.Integrity, sim.MaxIntegrity);
        }

        [Test]
        public void Pulse_DamagesPushesBackAndCoolsDown()
        {
            // Weaker Pulse so that the first Drifter survives and the knockback can be measured.
            RunConfig config = Config();
            config.PulseDamage = 5 * Hp;
            var sim = new GameSimulation(config, ContentDatabase.CreatePrototypeDefaults());
            sim.Enqueue(Command.Sell(0));
            sim.Enqueue(Command.StartWave());
            sim.Step();

            // Wait until the first enemy is inside Pulse range.
            while (!sim.IsOver)
            {
                bool inRange = false;
                foreach (Enemy e in sim.Enemies)
                {
                    inRange |= e.Radius <= sim.Config.PulseRadius;
                }

                if (inRange)
                {
                    break;
                }

                sim.Step();
            }

            Enemy target = sim.Enemies[0];
            long radiusBefore = target.Radius;
            long hpBefore = target.Hp;
            sim.Enqueue(Command.Pulse());
            sim.ApplyPendingCommandsNow();

            Assert.Less(target.Hp, hpBefore);
            Assert.Greater(target.Radius, radiusBefore);
            Assert.AreEqual(CommandResult.PulseNotReady, sim.Validate(Command.Pulse()));
        }

        [Test]
        public void Interest_IsPaidOnSavedCredits()
        {
            GameSimulation sim = Create(credits: 12);
            sim.Enqueue(Command.StartWave());
            var events = new List<SimEvent>();
            while (sim.WavesCleared == 0 && !sim.IsOver)
            {
                sim.Step();
            }

            sim.DrainEvents(events);
            SimEvent cleared = events.Find(e => e.Type == SimEventType.WaveCleared);
            Assert.AreEqual(2, cleared.Extra, "12 credits -> 2 interest");
            Assert.AreEqual(12 + 4 + 2, sim.Credits);
        }

        [Test]
        public void SameSeedAndCommands_ProduceIdenticalState()
        {
            Assert.AreEqual(PlayWithBot(7).ComputeStateHash(), PlayWithBot(7).ComputeStateHash());
            Assert.AreNotEqual(PlayWithBot(7).ComputeStateHash(), PlayWithBot(8).ComputeStateHash());
        }

        [Test]
        public void Replay_RoundTripsAndVerifies()
        {
            GameSimulation played = PlayWithBot(11);
            Assert.IsTrue(played.IsOver, "bot run should finish");

            Replay replay = Replay.Deserialize(Replay.Record(played).Serialize());
            ReplayCheck check = ReplayVerifier.Verify(replay, ContentDatabase.CreatePrototypeDefaults(), () => Config());
            Assert.IsTrue(check.IsValid, check.Reason);
        }

        [Test]
        public void Replay_TamperedCommandsAreRejected()
        {
            Replay replay = Replay.Record(PlayWithBot(11));
            int firstBuy = replay.Entries.FindIndex(e => e.Command.Type == CommandType.Buy);
            Assert.GreaterOrEqual(firstBuy, 0);
            ReplayEntry original = replay.Entries[firstBuy];
            replay.Entries[firstBuy] = new ReplayEntry(original.Tick, Command.Reroll());

            ReplayCheck check = ReplayVerifier.Verify(replay, ContentDatabase.CreatePrototypeDefaults(), () => Config());
            Assert.IsFalse(check.IsValid);
        }

        [Test]
        public void Undo_RevertsBuySellAndMove_ButNotReroll()
        {
            GameSimulation sim = Create(credits: 500);
            ulong initial = sim.ComputeStateHash();
            int amp = RerollUntil(sim, ModuleKind.Amplifier);
            Assert.IsFalse(sim.CanUndo, "reroll clears the undo stack");
            int creditsBefore = sim.Credits;

            sim.Enqueue(Command.Buy(amp, 1));
            sim.Enqueue(Command.Move(1, 3));
            sim.ApplyPendingCommandsNow();
            Assert.AreEqual(ModuleKind.Amplifier, sim.Ring.At(3).Kind);

            sim.Enqueue(Command.Undo());
            sim.ApplyPendingCommandsNow();
            Assert.AreEqual(ModuleKind.Amplifier, sim.Ring.At(1).Kind, "move undone");

            sim.Enqueue(Command.Undo());
            sim.ApplyPendingCommandsNow();
            Assert.IsNull(sim.Ring.At(1), "buy undone");
            Assert.AreEqual(creditsBefore, sim.Credits);
            Assert.AreEqual(ModuleKind.Amplifier, sim.OfferAt(amp), "offer is back");
            Assert.AreEqual(CommandResult.NothingToUndo, sim.Validate(Command.Undo()));
            Assert.AreNotEqual(initial, sim.ComputeStateHash(), "rerolls are not reverted");
        }

        [Test]
        public void Preview_MatchesTheRealOutcome()
        {
            GameSimulation sim = Create(credits: 500);
            long dpsBefore = sim.RingDps();
            int amp = RerollUntil(sim, ModuleKind.Amplifier);

            Assert.IsTrue(sim.TryPreviewBuy(amp, 1, out long predicted, out bool merges));
            Assert.IsFalse(merges);
            Assert.Greater(predicted, dpsBefore);

            sim.Enqueue(Command.Buy(amp, 1));
            sim.ApplyPendingCommandsNow();
            Assert.AreEqual(predicted, sim.RingDps(), "preview == actual");

            Assert.IsTrue(sim.TryPreviewMove(1, 3, out long afterMove));
            Assert.AreEqual(dpsBefore, afterMove, "amplifier away from the emitter");
            Assert.IsTrue(sim.TryPreviewSell(0, out long afterSell, out int refund));
            Assert.AreEqual(0, afterSell);
            Assert.AreEqual(1, refund);
        }

        [Test]
        public void WaveGrowth_MatchesGdd()
        {
            var director = new WaveDirector(new RunConfig(), ContentDatabase.CreatePrototypeDefaults(), new Pcg32(1, 1));
            Assert.AreEqual(22.19, director.HpMultiplierPpm(18) / 1_000_000.0, 0.3);
            Assert.AreEqual(8 * 5.05, director.BudgetMilli(18) / 1000.0, 0.5);
            Assert.IsTrue(director.IsGuardianWave(6));
        }

        private static GameSimulation PlayWithBot(ulong seed, BotStrategy strategy = BotStrategy.Naive)
        {
            GameSimulation sim = Create(seed);
            BalanceBot.Play(sim, strategy);
            return sim;
        }

        [Test]
        public void Bots_AreDeterministicForEveryStrategy()
        {
            foreach (BotStrategy strategy in System.Enum.GetValues(typeof(BotStrategy)))
            {
                RunResult first = BalanceBot.Play(Config(21), ContentDatabase.CreatePrototypeDefaults(), strategy);
                RunResult second = BalanceBot.Play(Config(21), ContentDatabase.CreatePrototypeDefaults(), strategy);
                Assert.AreEqual(first.FinalHash, second.FinalHash, strategy.ToString());
                Assert.IsTrue(first.Won || first.DefeatedBy.HasValue, $"{strategy}: the run must end");
            }
        }

        [Test]
        public void GreedyBot_NeverLowersRingDps()
        {
            // Every greedy shop action is chosen from a preview with a positive gain, so DPS is monotonic per shop.
            GameSimulation sim = Create(5, credits: 30);
            long before = sim.RingDps();
            BalanceBot.Play(sim, BotStrategy.MaxDps);
            Assert.GreaterOrEqual(sim.RingDps(), before);
        }

        [Test]
        public void BalanceRunner_ReportsEverySeedAndStrategy()
        {
            BalanceReport report = BalanceRunner.Run(() => new RunConfig(), ContentDatabase.CreatePrototypeDefaults(),
                new[] { BotStrategy.Naive, BotStrategy.MaxDps }, firstSeed: 100, seedCount: 3);
            Assert.AreEqual(6, report.Rows.Count);
            string csv = report.ToCsv();
            Assert.AreEqual(7, csv.Split('\n').Length - 1, "header + 6 rows");
            StringAssert.Contains("Naive:", report.Summary());
            StringAssert.Contains("MaxDps:", report.Summary());
        }
    }
}
