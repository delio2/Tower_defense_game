using System;
using System.Collections.Generic;

namespace TowerDefense.Simulation
{
    /// <summary>How a bot plays the shop. All strategies share the same wave policy (see <see cref="BalanceBot"/>).</summary>
    public enum BotStrategy : byte
    {
        /// <summary>Buys every affordable offer in order into the first free slot; Pulse when enemies are close.</summary>
        Naive = 0,

        /// <summary>Greedy: the purchase or swap with the best DPS gain per Credit, using the simulation previews.</summary>
        MaxDps = 1,

        /// <summary>Economy modules first, keeps a reserve for the interest cap, spends everything before a Guardian.</summary>
        EconomyFirst = 2,

        /// <summary>Archetype "swarm" (docs/07 §2.2): greedy, but multi-target weapons and their boosters weigh double.</summary>
        Swarm = 3,

        /// <summary>Archetype "sniper": greedy, but heavy, long-range weapons and Lens/Amplifier weigh double.</summary>
        Sniper = 4,

        /// <summary>Archetype "fortress": Bulwark, Frost and Capacitor first, then greedy; Pulses at the first enemy in reach.</summary>
        Fortress = 5,

        /// <summary>
        /// One-wave lookahead: at every shop it copies the run, plays the shop with each archetype, plays the next wave
        /// on every copy and keeps the plan that survives with the most Integrity and damage. Slow (it re-simulates the
        /// run once per candidate per shop): meant for the command-line farm, Tools/BotFarm.
        /// </summary>
        Planner = 6,
    }

    /// <summary>Outcome of one bot run, flattened for reports (see <see cref="BalanceRunner"/>).</summary>
    public sealed class RunResult
    {
        public ulong Seed;
        public BotStrategy Strategy;
        public bool Won;
        public int WavesCleared;
        public long Ticks;
        public long TotalDamage;
        public int Kills;
        public int CreditsLeft;
        public int Purchases;
        public int Merges;
        public int Rerolls;
        public int PulseUses;
        public EnemyKind? DefeatedBy;
        public ulong FinalHash;

        /// <summary>The ring at the end, slot by slot ("Emitter3 Amplifier2 - ..."), to spot build archetypes.</summary>
        public string FinalRing;

        /// <summary>Planner only: the archetype chosen at each shop, one letter per shop (M S N F E).</summary>
        public string Plan = string.Empty;

        /// <summary>Average combat seconds per wave played (the shop is timeless).</summary>
        public double SecondsPerWave => Ticks / (double)SimConstants.TicksPerSecond / (WavesCleared + (Won ? 0 : 1));
    }

    /// <summary>
    /// Deterministic bots that play whole runs through the public command API only. They are the first balance
    /// probe (GDD v0.2 §16, §18): thousands of runs in seconds, before any human tester.
    /// </summary>
    public static class BalanceBot
    {
        private const int MaxSteps = 400_000;
        private const int MaxShopActions = 64;

        /// <summary>Credits kept untouched by <see cref="BotStrategy.EconomyFirst"/> to farm the interest cap.</summary>
        private const int EconomyReserveFactor = 1;

        public static RunResult Play(RunConfig config, ContentDatabase content, BotStrategy strategy)
        {
            return Play(new GameSimulation(config, content), strategy);
        }

        public static RunResult Play(GameSimulation sim, BotStrategy strategy)
        {
            var result = new RunResult { Seed = sim.Config.Seed, Strategy = strategy };
            var events = new List<SimEvent>();

            for (int guard = 0; guard < MaxSteps && !sim.IsOver; guard++)
            {
                if (sim.Phase == GamePhase.Shop)
                {
                    if (strategy == BotStrategy.Planner)
                    {
                        result.Plan += PlanLetter(PlayPlannerShop(sim));
                    }
                    else
                    {
                        PlayShop(sim, strategy);
                    }

                    sim.Enqueue(Command.StartWave());
                    sim.ApplyPendingCommandsNow();
                }
                else
                {
                    if (sim.IsPulseReady && ShouldPulse(sim, strategy))
                    {
                        sim.Enqueue(Command.Pulse());
                    }

                    sim.Step();
                }

                sim.DrainEvents(events);
                Count(events, result);
                events.Clear();
            }

            result.Won = sim.HasWon;
            result.WavesCleared = sim.WavesCleared;
            result.Ticks = sim.Tick;
            result.TotalDamage = sim.TotalDamage;
            result.Kills = sim.Kills;
            result.CreditsLeft = sim.Credits;
            result.DefeatedBy = sim.DefeatedBy;
            result.FinalHash = sim.ComputeStateHash();
            result.FinalRing = DescribeRing(sim.Ring);
            return result;
        }

        private static string DescribeRing(Ring ring)
        {
            var parts = new List<string>(ring.SlotCount);
            for (int slot = 0; slot < ring.SlotCount; slot++)
            {
                ModuleInstance module = ring.At(slot);
                parts.Add(module == null ? "-" : module.Kind + module.Level.ToString());
            }

            return string.Join(" ", parts);
        }

        // ---------------------------------------------------------------- wave policy

        /// <summary>The bots' wave policy, for callers that drive a live run (the autoplay benchmark).</summary>
        public static bool WantsPulse(GameSimulation sim, BotStrategy strategy) => sim.IsPulseReady && ShouldPulse(sim, strategy);

        private static bool ShouldPulse(GameSimulation sim, BotStrategy strategy)
        {
            long emergencyRadius = SimConstants.CoreRadius * 2;
            bool eager = strategy == BotStrategy.Naive || strategy == BotStrategy.Fortress;
            long threshold = strategy == BotStrategy.Naive ? sim.Config.PulseRadius / 2 : sim.Config.PulseRadius;
            int inRange = 0;
            foreach (Enemy enemy in sim.Enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (enemy.Radius <= emergencyRadius)
                {
                    return true;
                }

                if (enemy.Radius <= threshold)
                {
                    inRange++;
                }
            }

            // Naive and Fortress pulse at the first enemy in reach; the others wait for a group (GDD v0.2 §4).
            return eager ? inRange > 0 : inRange >= 3;
        }

        // ---------------------------------------------------------------- shop policies

        /// <summary>Plays the current shop visit with a strategy (without starting the wave).</summary>
        public static void PlayShop(GameSimulation sim, BotStrategy strategy)
        {
            switch (strategy)
            {
                case BotStrategy.Naive:
                    PlayNaiveShop(sim);
                    break;
                case BotStrategy.MaxDps:
                    PlayGreedyShop(sim, reserve: 0, economyFirst: false);
                    break;
                case BotStrategy.EconomyFirst:
                    int reserve = sim.IsGuardianWave || sim.WavesCleared < 1
                        ? 0
                        : sim.Config.InterestStep * (sim.Config.InterestCap + sim.Ring.InterestCapBonus) * EconomyReserveFactor;
                    PlayGreedyShop(sim, reserve, economyFirst: true);
                    break;
                case BotStrategy.Swarm:
                    PlayGreedyShop(sim, reserve: 0, economyFirst: false, weights: SwarmWeights);
                    break;
                case BotStrategy.Sniper:
                    PlayGreedyShop(sim, reserve: 0, economyFirst: false, weights: SniperWeights);
                    break;
                case BotStrategy.Fortress:
                    PlayGreedyShop(sim, reserve: 0, economyFirst: false, supportFirst: FortressModules);
                    break;
                case BotStrategy.Planner:
                    PlayPlannerShop(sim);
                    break;
            }
        }

        // ---------------------------------------------------------------- archetypes

        /// <summary>Permille weight of each module's DPS gain per Credit; 1000 when a kind is not listed.</summary>
        private static int[] Weights(params (ModuleKind Kind, int Permille)[] entries)
        {
            var weights = new int[32];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = 1000;
            }

            foreach ((ModuleKind kind, int permille) in entries)
            {
                weights[(int)kind] = permille;
            }

            return weights;
        }

        private static readonly int[] SwarmWeights = Weights(
            (ModuleKind.Scatter, 2000), (ModuleKind.Arc, 2000), (ModuleKind.Echo, 1500), (ModuleKind.Overclock, 1500),
            (ModuleKind.Lance, 400), (ModuleKind.Mortar, 400));

        private static readonly int[] SniperWeights = Weights(
            (ModuleKind.Lance, 2000), (ModuleKind.Mortar, 2000), (ModuleKind.Lens, 1500), (ModuleKind.Amplifier, 1500),
            (ModuleKind.Scatter, 400), (ModuleKind.Arc, 400));

        private static readonly ModuleKind[] FortressModules = { ModuleKind.Bulwark, ModuleKind.Frost, ModuleKind.Capacitor };

        // ---------------------------------------------------------------- planner (one-wave lookahead)

        private static readonly BotStrategy[] PlannerCandidates =
        {
            BotStrategy.MaxDps, BotStrategy.Swarm, BotStrategy.Sniper, BotStrategy.Fortress, BotStrategy.EconomyFirst,
        };

        /// <summary>Wave time the lookahead may simulate before giving up (a wave lasts about 30 s).</summary>
        private const int MaxLookaheadTicks = 180 * SimConstants.TicksPerSecond;

        /// <summary>Plays the shop with the candidate whose copy of the run does best in the next wave; returns it.</summary>
        private static BotStrategy PlayPlannerShop(GameSimulation sim)
        {
            Replay history = Replay.Record(sim);
            BotStrategy best = BotStrategy.MaxDps;
            long bestScore = long.MinValue;
            foreach (BotStrategy candidate in PlannerCandidates)
            {
                GameSimulation copy = Clone(history, sim);
                PlayShop(copy, candidate);
                long score = ScoreNextWave(copy, candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            PlayShop(sim, best);
            return best;
        }

        /// <summary>An independent copy of a run: its replay so far, re-simulated on a fresh simulation.</summary>
        internal static GameSimulation Clone(GameSimulation sim) => Clone(Replay.Record(sim), sim);

        private static GameSimulation Clone(Replay history, GameSimulation sim)
        {
            return ReplayVerifier.Resimulate(history, new GameSimulation(sim.Config.Copy(), sim.Content));
        }

        /// <summary>
        /// Survival first, then Integrity kept (in 5-point steps, so noise does not decide), then the ring's DPS plus
        /// the Credits in hand at a rough exchange rate of 1 Credit = 1/40 of the ring's DPS (estimate).
        /// </summary>
        private static long ScoreNextWave(GameSimulation sim, BotStrategy strategy)
        {
            long dps = sim.RingDps();
            int credits = sim.Credits;
            sim.Enqueue(Command.StartWave());
            sim.ApplyPendingCommandsNow();
            for (int tick = 0; tick < MaxLookaheadTicks && sim.Phase == GamePhase.Wave; tick++)
            {
                if (sim.IsPulseReady && ShouldPulse(sim, strategy))
                {
                    sim.Enqueue(Command.Pulse());
                }

                sim.Step();
            }

            if (sim.Phase == GamePhase.Defeat)
            {
                return long.MinValue / 2 + sim.WaveEnemiesLeft * -1L;
            }

            long integritySteps = sim.Integrity / (5L * SimConstants.HpScale);
            long strength = Math.Min(dps + credits * Math.Max(1, dps / 40), (1L << 40) - 1);
            return (integritySteps << 40) + strength;
        }

        private static char PlanLetter(BotStrategy strategy) => strategy switch
        {
            BotStrategy.Swarm => 'S',
            BotStrategy.Sniper => 'N',
            BotStrategy.Fortress => 'F',
            BotStrategy.EconomyFirst => 'E',
            _ => 'M',
        };

        private static void PlayNaiveShop(GameSimulation sim)
        {
            for (int offer = 0; offer < sim.OfferCount; offer++)
            {
                int slot = FirstFreeSlot(sim);
                if (sim.Validate(Command.Buy(offer, slot)) == CommandResult.Ok)
                {
                    sim.Enqueue(Command.Buy(offer, slot));
                    sim.ApplyPendingCommandsNow();
                }
            }
        }

        /// <summary>
        /// Repeats "best action per Credit" until nothing improves: buy (DPS gain per Credit), swap (free DPS gain),
        /// economy modules when asked, and one reroll when the shop offers nothing useful and Credits allow it.
        /// </summary>
        private static void PlayGreedyShop(GameSimulation sim, int reserve, bool economyFirst, int[] weights = null, ModuleKind[] supportFirst = null)
        {
            bool rerolled = false;
            for (int action = 0; action < MaxShopActions; action++)
            {
                if (TryBestSwap(sim))
                {
                    continue;
                }

                int budget = sim.Credits - reserve;
                if (economyFirst && TryBuyEconomy(sim, budget))
                {
                    continue;
                }

                if (supportFirst != null && TryBuyKinds(sim, budget, supportFirst, maxSlots: 2))
                {
                    continue;
                }

                if (TryBuyExtraSlot(sim, budget))
                {
                    continue;
                }

                if (TryBestBuy(sim, budget, weights))
                {
                    continue;
                }

                if (!rerolled && sim.RerollCost + 3 <= budget && sim.Validate(Command.Reroll()) == CommandResult.Ok)
                {
                    rerolled = true;
                    sim.Enqueue(Command.Reroll());
                    sim.ApplyPendingCommandsNow();
                    continue;
                }

                break;
            }
        }

        private static bool TryBestSwap(GameSimulation sim)
        {
            long before = sim.RingDps();
            long bestGain = 0;
            int bestFrom = -1, bestTo = -1;
            for (int from = 0; from < sim.Ring.SlotCount; from++)
            {
                if (sim.Ring.At(from) == null)
                {
                    continue;
                }

                for (int to = from + 1; to < sim.Ring.SlotCount; to++)
                {
                    if (sim.TryPreviewMove(from, to, out long after) && after - before > bestGain)
                    {
                        bestGain = after - before;
                        bestFrom = from;
                        bestTo = to;
                    }
                }
            }

            if (bestFrom < 0)
            {
                return false;
            }

            sim.Enqueue(Command.Move(bestFrom, bestTo));
            sim.ApplyPendingCommandsNow();
            return true;
        }

        /// <summary>Opens an extra slot only when the ring is full and there is money left to fill it (cost + 3).</summary>
        private static bool TryBuyExtraSlot(GameSimulation sim, int budget)
        {
            if (sim.Ring.HasFreeSlot() || !sim.CanBuyExtraSlot || sim.ExtraSlotCost + 3 > budget)
            {
                return false;
            }

            long bestDps = -1;
            int bestIndex = -1;
            for (int insertAt = 0; insertAt <= sim.Ring.SlotCount; insertAt++)
            {
                if (sim.TryPreviewBuySlot(insertAt, out long dps) && dps > bestDps)
                {
                    bestDps = dps;
                    bestIndex = insertAt;
                }
            }

            if (bestIndex < 0)
            {
                return false;
            }

            sim.Enqueue(Command.BuySlot(bestIndex));
            sim.ApplyPendingCommandsNow();
            return true;
        }

        private static bool TryBestBuy(GameSimulation sim, int budget, int[] weights = null)
        {
            long before = sim.RingDps();
            long bestScore = 0;
            int bestOffer = -1, bestSlot = -1;
            for (int offer = 0; offer < sim.OfferCount; offer++)
            {
                ModuleKind? kind = sim.OfferAt(offer);
                if (kind == null)
                {
                    continue;
                }

                int cost = sim.Content.Module(kind.Value).Cost;
                if (cost > budget)
                {
                    continue;
                }

                for (int slot = 0; slot < sim.Ring.SlotCount; slot++)
                {
                    if (!sim.TryPreviewBuy(offer, slot, out long after, out bool merges))
                    {
                        continue;
                    }

                    // Gain per Credit, scaled to keep integers; ties favour the cheaper purchase.
                    long score = (after - before) * (weights == null ? 1000 : weights[(int)kind.Value]) / cost;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestOffer = offer;
                        bestSlot = slot;
                    }

                    if (merges)
                    {
                        break; // the slot is ignored when merging
                    }
                }
            }

            if (bestOffer < 0)
            {
                return false;
            }

            sim.Enqueue(Command.Buy(bestOffer, bestSlot));
            sim.ApplyPendingCommandsNow();
            return true;
        }

        /// <summary>Buys the first affordable economy module when the ring still has room for it (at most 2 economy slots).</summary>
        private static bool TryBuyEconomy(GameSimulation sim, int budget)
        {
            int economySlots = 0;
            for (int slot = 0; slot < sim.Ring.SlotCount; slot++)
            {
                ModuleInstance module = sim.Ring.At(slot);
                if (module != null && module.Category == ModuleCategory.Economy)
                {
                    economySlots++;
                }
            }

            for (int offer = 0; offer < sim.OfferCount; offer++)
            {
                ModuleKind? kind = sim.OfferAt(offer);
                if (kind == null)
                {
                    continue;
                }

                ModuleDefinition definition = sim.Content.Module(kind.Value);
                if (definition.Category != ModuleCategory.Economy || definition.Cost > budget)
                {
                    continue;
                }

                bool merges = sim.Ring.FindMergeTarget(kind.Value) != null;
                if (!merges && economySlots >= 2)
                {
                    continue;
                }

                int slot = FirstFreeSlot(sim);
                if (sim.Validate(Command.Buy(offer, slot)) != CommandResult.Ok)
                {
                    continue;
                }

                sim.Enqueue(Command.Buy(offer, slot));
                sim.ApplyPendingCommandsNow();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Buys (or merges) the first affordable offer among <paramref name="kinds"/> while fewer than
        /// <paramref name="maxSlots"/> ring slots hold them. These modules add no DPS, so the greedy buyer never picks them.
        /// </summary>
        private static bool TryBuyKinds(GameSimulation sim, int budget, ModuleKind[] kinds, int maxSlots)
        {
            int owned = 0;
            for (int slot = 0; slot < sim.Ring.SlotCount; slot++)
            {
                ModuleInstance module = sim.Ring.At(slot);
                if (module != null && Array.IndexOf(kinds, module.Kind) >= 0)
                {
                    owned++;
                }
            }

            for (int offer = 0; offer < sim.OfferCount; offer++)
            {
                ModuleKind? kind = sim.OfferAt(offer);
                if (kind == null || Array.IndexOf(kinds, kind.Value) < 0 || sim.Content.Module(kind.Value).Cost > budget)
                {
                    continue;
                }

                if (sim.Ring.FindMergeTarget(kind.Value) == null && owned >= maxSlots)
                {
                    continue;
                }

                int slot = FirstFreeSlot(sim);
                if (sim.Validate(Command.Buy(offer, slot)) != CommandResult.Ok)
                {
                    continue;
                }

                sim.Enqueue(Command.Buy(offer, slot));
                sim.ApplyPendingCommandsNow();
                return true;
            }

            return false;
        }

        private static int FirstFreeSlot(GameSimulation sim)
        {
            for (int slot = 0; slot < sim.Ring.SlotCount; slot++)
            {
                if (sim.Ring.At(slot) == null)
                {
                    return slot;
                }
            }

            return 0;
        }

        private static void Count(List<SimEvent> events, RunResult result)
        {
            foreach (SimEvent e in events)
            {
                switch (e.Type)
                {
                    case SimEventType.ModuleBought:
                        result.Purchases++;
                        break;
                    case SimEventType.ModuleMerged:
                        result.Purchases++;
                        result.Merges++;
                        break;
                    case SimEventType.Rerolled:
                        result.Rerolls++;
                        break;
                    case SimEventType.PulseUsed:
                        result.PulseUses++;
                        break;
                }
            }
        }
    }
}
