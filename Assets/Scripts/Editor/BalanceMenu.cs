using System;
using System.Diagnostics;
using System.IO;
using TowerDefense.Simulation;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace TowerDefense.Editor
{
    /// <summary>
    /// Runs the balance bots from the editor menu and writes a CSV under Temp/Balance (never inside Assets).
    /// The summary goes to the console so it can be pasted into docs/05 §18.
    /// </summary>
    public static class BalanceMenu
    {
        private const string OutputFolder = "Temp/Balance";
        private static readonly BotStrategy[] AllStrategies = { BotStrategy.Naive, BotStrategy.MaxDps, BotStrategy.EconomyFirst };

        [MenuItem("TowerDefense/Balance/Run bots (100 seeds)")]
        public static void Run100() => Run(100);

        [MenuItem("TowerDefense/Balance/Run bots (1000 seeds)")]
        public static void Run1000() => Run(1000);

        private static void Run(int seeds)
        {
            var stopwatch = Stopwatch.StartNew();
            BalanceReport report = BalanceRunner.Run(() => new RunConfig(), ContentDatabase.CreatePrototypeDefaults(), AllStrategies, firstSeed: 1, seedCount: seeds);
            stopwatch.Stop();

            Directory.CreateDirectory(OutputFolder);
            string path = Path.Combine(OutputFolder, $"bots-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
            File.WriteAllText(path, report.ToCsv());

            // One log entry per line: the console (and MCP) show only the first line of a multi-line message.
            Debug.Log($"Balance bots: {report.Rows.Count} runs in {stopwatch.Elapsed.TotalSeconds:F1} s (balance {RunConfig.BalanceVersion}), CSV: {path}");
            foreach (string line in report.Summary().Split('\n'))
            {
                if (line.Trim().Length > 0)
                {
                    Debug.Log(line.TrimEnd());
                }
            }
        }
    }
}
