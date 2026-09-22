using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TowerDefense.Simulation;

// Headless balance farm (docs/06 §4). Plays seeds × strategies in parallel with the same Simulation code the game uses,
// writes a CSV and a summary to Temp/Balance, and prints the summary.
//
//   dotnet run -c Release --project Tools/BotFarm -- --seeds 1000 --strategies all --core Standard --grade 0
//
// Options: --seeds N (default 500) · --first S (first seed, default 1) · --strategies all|fast|Name,Name
// (fast = everything but Planner) · --core Standard|Merchant|Bastion|Glass · --grade 0-3 · --out folder.
internal static class Program
{
    private static int Main(string[] args)
    {
        Dictionary<string, string> options = ParseArgs(args);
        int seeds = int.Parse(Get(options, "seeds", "500"), CultureInfo.InvariantCulture);
        ulong first = ulong.Parse(Get(options, "first", "1"), CultureInfo.InvariantCulture);
        CoreType core = Enum.Parse<CoreType>(Get(options, "core", "Standard"), ignoreCase: true);
        int grade = int.Parse(Get(options, "grade", "0"), CultureInfo.InvariantCulture);
        string outFolder = Get(options, "out", Path.Combine("Temp", "Balance"));
        BotStrategy[] strategies = ParseStrategies(Get(options, "strategies", "fast"));

        var jobs = new List<(BotStrategy Strategy, ulong Seed)>();
        foreach (BotStrategy strategy in strategies)
        {
            for (int i = 0; i < seeds; i++)
            {
                jobs.Add((strategy, first + (ulong)i));
            }
        }

        Console.WriteLine($"Bot farm: {jobs.Count} runs ({seeds} seeds x {string.Join(", ", strategies)}), core {core}, grade {grade}, balance {RunConfig.BalanceVersion}, {Environment.ProcessorCount} threads");
        var results = new RunResult[jobs.Count];
        var content = new ThreadLocal<ContentDatabase>(ContentDatabase.CreatePrototypeDefaults);
        int done = 0, lastPercent = -1;
        var stopwatch = Stopwatch.StartNew();

        // Slow jobs (Planner) first, so the tail of the run is not one core grinding alone.
        int[] order = Enumerable.Range(0, jobs.Count).OrderBy(i => jobs[i].Strategy == BotStrategy.Planner ? 0 : 1).ToArray();
        Parallel.ForEach(order, index =>
        {
            (BotStrategy strategy, ulong seed) = jobs[index];
            RunConfig config = RunSetup.Create(seed, core, grade);
            results[index] = BalanceBot.Play(config, content.Value, strategy);
            int finished = Interlocked.Increment(ref done);
            int percent = finished * 100 / jobs.Count;
            if (percent % 10 == 0 && Interlocked.Exchange(ref lastPercent, percent) != percent)
            {
                Console.WriteLine($"  {percent}% ({finished}/{jobs.Count}) {stopwatch.Elapsed.TotalSeconds:F0} s");
            }
        });
        stopwatch.Stop();

        var report = new BalanceReport();
        report.Rows.AddRange(results);
        string summary = report.Summary() + Builds(results) + Plans(results);

        Directory.CreateDirectory(outFolder);
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        string name = $"farm-{core}-g{grade}-{stamp}";
        File.WriteAllText(Path.Combine(outFolder, name + ".csv"), report.ToCsv());
        File.WriteAllText(Path.Combine(outFolder, name + ".txt"),
            $"{jobs.Count} runs in {stopwatch.Elapsed.TotalSeconds:F1} s, core {core}, grade {grade}, balance {RunConfig.BalanceVersion}\n{summary}");

        Console.WriteLine($"Done in {stopwatch.Elapsed.TotalSeconds:F1} s -> {Path.Combine(outFolder, name)}.csv/.txt");
        Console.WriteLine(summary);
        return 0;
    }

    /// <summary>The most common winning weapon sets per strategy: are different builds winning? (Gate 2, docs/06).</summary>
    private static string Builds(RunResult[] results)
    {
        var sb = new StringBuilder("\nWinning weapon sets (top 5 per strategy):\n");
        foreach (IGrouping<BotStrategy, RunResult> group in results.Where(r => r.Won).GroupBy(r => r.Strategy))
        {
            var sets = group
                .Select(r => string.Join("+", r.FinalRing.Split(' ')
                    .Where(p => p != "-")
                    .Select(p => p.TrimEnd('1', '2', '3'))
                    .Where(IsWeapon)
                    .Distinct()
                    .OrderBy(p => p, StringComparer.Ordinal)))
                .GroupBy(s => s)
                .OrderByDescending(g => g.Count())
                .ToList();
            sb.Append($"  {group.Key}: {sets.Count} distinct sets among {group.Count()} wins; ");
            sb.AppendLine(string.Join(" | ", sets.Take(5).Select(g => $"{g.Key} x{g.Count()}")));
        }

        return sb.ToString();
    }

    /// <summary>For the Planner: which archetype it chose, shop by shop, over all its runs.</summary>
    private static string Plans(RunResult[] results)
    {
        RunResult[] planner = results.Where(r => r.Strategy == BotStrategy.Planner).ToArray();
        if (planner.Length == 0)
        {
            return string.Empty;
        }

        var counts = planner.SelectMany(r => r.Plan).GroupBy(c => c).OrderByDescending(g => g.Count());
        int total = planner.Sum(r => r.Plan.Length);
        return "\nPlanner choices per shop (M MaxDps, S Swarm, N Sniper, F Fortress, E EconomyFirst): "
            + string.Join(" ", counts.Select(g => $"{g.Key} {g.Count() * 100.0 / total:F0}%")) + "\n";
    }

    private static bool IsWeapon(string kind) =>
        Enum.TryParse(kind, out ModuleKind k) && (int)k < 10;

    private static BotStrategy[] ParseStrategies(string value)
    {
        BotStrategy[] all = (BotStrategy[])Enum.GetValues(typeof(BotStrategy));
        return value.ToLowerInvariant() switch
        {
            "all" => all,
            "fast" => all.Where(s => s != BotStrategy.Planner).ToArray(),
            _ => value.Split(',').Select(s => Enum.Parse<BotStrategy>(s.Trim(), ignoreCase: true)).ToArray(),
        };
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].StartsWith("--", StringComparison.Ordinal))
            {
                options[args[i].Substring(2)] = args[i + 1];
                i++;
            }
        }

        return options;
    }

    private static string Get(Dictionary<string, string> options, string key, string fallback) =>
        options.TryGetValue(key, out string value) ? value : fallback;
}
