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
                    PlayShop(sim, strategy);
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

            result.Won = sim.Phase == GamePhase.Victory;
            result.WavesCleared = sim.WavesCleared;
            result.Ticks = sim.Tick;
            result.TotalDamage = sim.TotalDamage;
            result.Kills = sim.Kills;
            result.CreditsLeft = sim.Credits;
            result.DefeatedBy = sim.DefeatedBy;
            result.FinalHash = sim.ComputeStateHash();
            return result;
        }

        // ---------------------------------------------------------------- wave policy

        private static bool ShouldPulse(GameSimulation sim, BotStrategy strategy)
        {
            long emergencyRadius = SimConstants.CoreRadius * 2;
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

            // Naive pulses at the first enemy in reach; the others wait for a group (GDD v0.2 §4).
            return strategy == BotStrategy.Naive ? inRange > 0 : inRange >= 3;
        }

        // ---------------------------------------------------------------- shop policies

        private static void PlayShop(GameSimulation sim, BotStrategy strategy)
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
            }
        }

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
        private static void PlayGreedyShop(GameSimulation sim, int reserve, bool economyFirst)
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

                if (TryBestBuy(sim, budget))
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

        private static bool TryBestBuy(GameSimulation sim, int budget)
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
                    long score = (after - before) * 1000 / cost;
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
