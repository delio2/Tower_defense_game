using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TowerDefense.Simulation
{
    /// <summary>
    /// Plays many seeds with several bots and aggregates the outcome. Pure C#: the editor menu and, later,
    /// a command-line tool only add file output around it.
    /// </summary>
    public static class BalanceRunner
    {
        public static BalanceReport Run(Func<RunConfig> configFactory, ContentDatabase content, IReadOnlyList<BotStrategy> strategies, ulong firstSeed, int seedCount)
        {
            var report = new BalanceReport();
            foreach (BotStrategy strategy in strategies)
            {
                for (int i = 0; i < seedCount; i++)
                {
                    RunConfig config = configFactory();
                    config.Seed = firstSeed + (ulong)i;
                    report.Rows.Add(BalanceBot.Play(config, content, strategy));
                }
            }

            return report;
        }
    }

    /// <summary>Rows of a balance run plus the aggregations used to read them.</summary>
    public sealed class BalanceReport
    {
        public readonly List<RunResult> Rows = new List<RunResult>();

        public string ToCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("seed,strategy,won,waves_cleared,ticks,seconds_per_wave,total_damage,kills,credits_left,purchases,merges,rerolls,pulse_uses,defeated_by,final_hash");
            foreach (RunResult r in Rows)
            {
                sb.Append(r.Seed.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(r.Strategy).Append(',')
                  .Append(r.Won ? 1 : 0).Append(',')
                  .Append(r.WavesCleared).Append(',')
                  .Append(r.Ticks.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(r.SecondsPerWave.ToString("F1", CultureInfo.InvariantCulture)).Append(',')
                  .Append(r.TotalDamage.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(r.Kills).Append(',')
                  .Append(r.CreditsLeft).Append(',')
                  .Append(r.Purchases).Append(',')
                  .Append(r.Merges).Append(',')
                  .Append(r.Rerolls).Append(',')
                  .Append(r.PulseUses).Append(',')
                  .Append(r.DefeatedBy.HasValue ? r.DefeatedBy.Value.ToString() : string.Empty).Append(',')
                  .Append(r.FinalHash.ToString(CultureInfo.InvariantCulture))
                  .AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>One block per strategy: win rate, where runs die, pacing, economy. Meant for the console.</summary>
        public string Summary()
        {
            var sb = new StringBuilder();
            foreach (BotStrategy strategy in Enum.GetValues(typeof(BotStrategy)))
            {
                var rows = Rows.FindAll(r => r.Strategy == strategy);
                if (rows.Count == 0)
                {
                    continue;
                }

                int wins = 0, totalWaves = 0, creditsLeft = 0, rerolls = 0, merges = 0, pulses = 0;
                double seconds = 0, damage = 0;
                var defeatWave = new SortedDictionary<int, int>();
                var defeatCause = new SortedDictionary<string, int>();
                foreach (RunResult r in rows)
                {
                    if (r.Won)
                    {
                        wins++;
                    }
                    else
                    {
                        int wave = r.WavesCleared + 1;
                        defeatWave[wave] = defeatWave.TryGetValue(wave, out int n) ? n + 1 : 1;
                        string cause = r.DefeatedBy?.ToString() ?? "?";
                        defeatCause[cause] = defeatCause.TryGetValue(cause, out int c) ? c + 1 : 1;
                    }

                    totalWaves += r.WavesCleared;
                    creditsLeft += r.CreditsLeft;
                    rerolls += r.Rerolls;
                    merges += r.Merges;
                    pulses += r.PulseUses;
                    seconds += r.SecondsPerWave;
                    damage += r.TotalDamage / (double)SimConstants.HpScale;
                }

                double n1 = rows.Count;
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0}: {1} runs, win {2:P0}, avg waves {3:F1}, {4:F1} s/wave, avg damage {5:N0}, credits left {6:F1}, rerolls {7:F1}, merges {8:F1}, pulses {9:F1}",
                    strategy, rows.Count, wins / n1, totalWaves / n1, seconds / n1, damage / n1, creditsLeft / n1, rerolls / n1, merges / n1, pulses / n1));

                if (defeatWave.Count > 0)
                {
                    sb.Append("  lost on wave: ");
                    foreach (var kv in defeatWave)
                    {
                        sb.Append(kv.Key).Append('=').Append(kv.Value).Append(' ');
                    }

                    sb.AppendLine();
                    sb.Append("  killed by: ");
                    foreach (var kv in defeatCause)
                    {
                        sb.Append(kv.Key).Append('=').Append(kv.Value).Append(' ');
                    }

                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}
