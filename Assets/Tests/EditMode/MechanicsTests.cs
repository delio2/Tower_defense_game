using System.Collections.Generic;
using NUnit.Framework;

namespace TowerDefense.Simulation.Tests
{
    /// <summary>
    /// Behaviour of the MVP modules and enemies added in Phase 2 (GDD v0.2 §6, §8). Enemies are placed at exact
    /// positions during an otherwise empty wave: slot 0 is at the top (direction Up) at ring radius 1.5.
    /// </summary>
    public class MechanicsTests
    {
        private const long Hp = SimConstants.HpScale;
        private const long Unit = SimConstants.Micro;
        private const int Up = Directions.Up;

        /// <summary>A run whose first wave spawns nothing, with the given modules mounted from slot 0.</summary>
        private static GameSimulation ArenaWith(params ModuleKind[] modules)
        {
            var config = new RunConfig { Seed = 3, StartingCredits = 0, FirstWaveBudgetMilli = 0, StartingModules = modules };
            var sim = new GameSimulation(config, ContentDatabase.CreatePrototypeDefaults());
            sim.Enqueue(Command.StartWave());
            sim.ApplyPendingCommandsNow();
            return sim;
        }

        private static List<SimEvent> StepAndCollect(GameSimulation sim, int ticks = 1)
        {
            var events = new List<SimEvent>();
            for (int i = 0; i < ticks; i++)
            {
                sim.Step();
            }

            sim.DrainEvents(events);
            return events;
        }

        private static List<SimEvent> Hits(List<SimEvent> events, SimEventType type = SimEventType.EnemyHit)
        {
            return events.FindAll(e => e.Type == type);
        }

        [Test]
        public void Arc_ChainsToNearbyEnemiesWithFalloff()
        {
            GameSimulation sim = ArenaWith(ModuleKind.Arc);
            Enemy first = sim.SpawnForTests(EnemyKind.Drifter, Up, 3 * Unit);
            Enemy second = sim.SpawnForTests(EnemyKind.Drifter, Up, 4 * Unit);   // 1.0 from the first: in jump range
            Enemy far = sim.SpawnForTests(EnemyKind.Drifter, Up, 6 * Unit);      // 2.0 from the second: out of jump range

            List<SimEvent> hits = Hits(StepAndCollect(sim));
            Assert.AreEqual(2, hits.Count, "chain reaches the second enemy but not the far one");
            Assert.AreEqual(first.Id, hits[0].EntityId);
            Assert.AreEqual(6 * Hp, hits[0].Value);
            Assert.AreEqual(second.Id, hits[1].EntityId);
            Assert.AreEqual(6 * Hp * 900 / 1000, hits[1].Value, "-10% per jump");
            Assert.AreEqual(far.MaxHp, far.Hp);
        }

        [Test]
        public void Lance_PiercesEveryEnemyOnTheLine()
        {
            GameSimulation sim = ArenaWith(ModuleKind.Lance);
            sim.SpawnForTests(EnemyKind.Drifter, Up, 3 * Unit);
            sim.SpawnForTests(EnemyKind.Drifter, Up, 5 * Unit);
            sim.SpawnForTests(EnemyKind.Drifter, Up, 7 * Unit);            // 5.5 from the module: within range 6.0
            Enemy aside = sim.SpawnForTests(EnemyKind.Drifter, Up + 24, 4 * Unit); // 45 degrees off the line

            List<SimEvent> hits = Hits(StepAndCollect(sim));
            Assert.AreEqual(3, hits.Count, "three on the line, the side enemy untouched");
            Assert.AreEqual(aside.MaxHp, aside.Hp);
            foreach (SimEvent hit in hits)
            {
                Assert.AreEqual(18 * Hp, hit.Value);
            }
        }

        [Test]
        public void Mortar_ExplodesOnTheFarthestEnemyInRange()
        {
            GameSimulation sim = ArenaWith(ModuleKind.Mortar);
            Enemy near = sim.SpawnForTests(EnemyKind.Drifter, Up, 3 * Unit);     // 1.5 from the module: under the 2.0 minimum
            Enemy target = sim.SpawnForTests(EnemyKind.Drifter, Up, 8 * Unit);   // 6.5 from the module: the farthest in range 7.0
            Enemy neighbour = sim.SpawnForTests(EnemyKind.Drifter, Up, 7 * Unit); // 1.0 from the target: inside the 1.2 splash

            List<SimEvent> hits = Hits(StepAndCollect(sim));
            Assert.AreEqual(2, hits.Count);
            CollectionAssert.AreEquivalent(new[] { target.Id, neighbour.Id }, hits.ConvertAll(h => h.EntityId));
            Assert.AreEqual(near.MaxHp, near.Hp, "too close for the mortar");
        }

        [Test]
        public void Echo_RepeatsNeighbourHitsAtHalfDamage()
        {
            GameSimulation sim = ArenaWith(ModuleKind.Emitter, ModuleKind.Echo);
            Assert.AreEqual(500, sim.Ring.At(0).EchoPermille);
            sim.SpawnForTests(EnemyKind.Drifter, Up, 3 * Unit);

            List<SimEvent> hits = Hits(StepAndCollect(sim));
            Assert.AreEqual(2, hits.Count);
            Assert.AreEqual(8 * Hp, hits[0].Value);
            Assert.AreEqual(4 * Hp, hits[1].Value);
        }

        [Test]
        public void Warden_ShieldsEnemiesAroundItButNotItself()
        {
            GameSimulation sim = ArenaWith(ModuleKind.Scatter);
            Enemy warden = sim.SpawnForTests(EnemyKind.Warden, Up, 3 * Unit);
            Enemy protectedDrifter = sim.SpawnForTests(EnemyKind.Drifter, Up, 4 * Unit);     // 1.0 from the Warden
            Enemy exposedDrifter = sim.SpawnForTests(EnemyKind.Drifter, Up + 24, 3 * Unit);  // 45 degrees away: 2.3 from the Warden

            List<SimEvent> hits = Hits(StepAndCollect(sim));
            Assert.AreEqual(3, hits.Count, "Scatter hits all three");
            long wardenHit = hits.Find(h => h.EntityId == warden.Id).Value;
            long shieldedHit = hits.Find(h => h.EntityId == protectedDrifter.Id).Value;
            long exposedHit = hits.Find(h => h.EntityId == exposedDrifter.Id).Value;
            Assert.AreEqual(5 * Hp - 1 * Hp, wardenHit, "the Warden takes full damage minus its armor");
            Assert.AreEqual(5 * Hp / 2, shieldedHit, "-50% inside the aura");
            Assert.AreEqual(5 * Hp, exposedHit);
        }

        [Test]
        public void Splitter_SplitsIntoTwoSwarmletsOnDeath()
        {
            GameSimulation sim = ArenaWith(ModuleKind.Lance);
            Enemy splitter = sim.SpawnForTests(EnemyKind.Splitter, Up, 3 * Unit);
            for (int i = 0; i < 400 && splitter.IsAlive; i++)
            {
                sim.Step();
            }

            Assert.IsFalse(splitter.IsAlive);
            var events = new List<SimEvent>();
            sim.DrainEvents(events);
            int swarmlets = events.FindAll(e => e.Type == SimEventType.EnemySpawned && e.Value == (long)EnemyKind.Swarmlet).Count;
            Assert.AreEqual(2, swarmlets);
        }

        [Test]
        public void Dasher_DashesAtTripleSpeedAtTheEndOfEveryCycle()
        {
            GameSimulation sim = ArenaWith();
            Enemy dasher = sim.SpawnForTests(EnemyKind.Dasher, Up, 8 * Unit);
            long before = dasher.Radius;
            sim.Step();
            long normalStep = before - dasher.Radius;
            Assert.AreEqual(SimConstants.SpeedPerTick(800), normalStep);
            Assert.IsFalse(dasher.IsDashing);

            for (int i = 0; i < 148; i++)
            {
                sim.Step();
            }

            Assert.IsFalse(dasher.IsDashing, "tick 149: still walking");
            before = dasher.Radius;
            sim.Step();
            Assert.IsTrue(dasher.IsDashing, "tick 150: the last 30 ticks of the 180-tick cycle");
            Assert.AreEqual(normalStep * 3, before - dasher.Radius);
        }

        [Test]
        public void Frost_SlowsEnemiesNearTheCore()
        {
            GameSimulation sim = ArenaWith(ModuleKind.Frost);
            Assert.AreEqual(250, sim.Ring.SlowPermille);
            Enemy far = sim.SpawnForTests(EnemyKind.Drifter, Up, 6 * Unit);
            Enemy near = sim.SpawnForTests(EnemyKind.Drifter, Up + 48, 2 * Unit);
            long farBefore = far.Radius, nearBefore = near.Radius;
            sim.Step();
            long step = SimConstants.SpeedPerTick(900);
            Assert.AreEqual(step, farBefore - far.Radius, "outside the 3.0 radius: full speed");
            Assert.AreEqual(step * 750 / 1000, nearBefore - near.Radius, "inside: -25%");
        }

        [Test]
        public void Capacitor_ShortensPulseCooldownAndRaisesItsDamage()
        {
            GameSimulation sim = ArenaWith(ModuleKind.Capacitor);
            Enemy drifter = sim.SpawnForTests(EnemyKind.Drifter, Up, 2 * Unit);
            sim.Enqueue(Command.Pulse());
            List<SimEvent> hits = Hits(StepAndCollect(sim));
            Assert.AreEqual(30 * Hp, hits.Find(h => h.EntityId == drifter.Id).Value, "20 x 1.5");
            Assert.AreEqual(1200 * 800 / 1000 - 1, sim.PulseCooldownRemaining, "-20%, minus the tick that already elapsed");
        }

        [Test]
        public void Salvage_PaysOneCreditPerTenKills()
        {
            GameSimulation sim = ArenaWith(ModuleKind.Salvage);
            Assert.AreEqual(100, sim.Ring.SalvagePermillePerKill);
        }

        [Test]
        public void Waves_UnlockEnemiesPerActAndAddElitesFromActTwo()
        {
            var config = new RunConfig { Seed = 5 };
            var director = new WaveDirector(config, ContentDatabase.CreatePrototypeDefaults(), Pcg32.ForStream(5, RngStream.Waves));
            var kindsByWave = new Dictionary<int, HashSet<EnemyKind>>();
            bool eliteInActOne = false, eliteLater = false;
            for (int wave = 1; wave <= 18; wave++)
            {
                List<SpawnEntry> spawns = director.BuildWave(wave);
                var kinds = new HashSet<EnemyKind>();
                foreach (SpawnEntry entry in spawns)
                {
                    kinds.Add(entry.Kind);
                    if (entry.IsElite)
                    {
                        if (wave <= 6) eliteInActOne = true; else eliteLater = true;
                    }
                }

                kindsByWave[wave] = kinds;
            }

            for (int wave = 1; wave <= 6; wave++)
            {
                Assert.IsFalse(kindsByWave[wave].Contains(EnemyKind.Dasher) || kindsByWave[wave].Contains(EnemyKind.Warden), $"wave {wave}");
            }

            Assert.AreEqual(EnemyKind.Dasher, director.BuildWave(7)[0].Kind, "wave 7 introduces the Dasher alone");
            Assert.IsFalse(eliteInActOne);
            Assert.IsTrue(eliteLater, "elites appear from act 2");
        }

        [Test]
        public void FullRun_ThreeActsEighteenWaves_IsDeterministic()
        {
            RunResult a = BalanceBot.Play(new RunConfig { Seed = 9 }, ContentDatabase.CreatePrototypeDefaults(), BotStrategy.MaxDps);
            RunResult b = BalanceBot.Play(new RunConfig { Seed = 9 }, ContentDatabase.CreatePrototypeDefaults(), BotStrategy.MaxDps);
            Assert.AreEqual(18, new RunConfig().TotalWaves);
            Assert.AreEqual(a.FinalHash, b.FinalHash);
            Assert.IsTrue(a.Won || a.DefeatedBy.HasValue);
        }
    }
}
