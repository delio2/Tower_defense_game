using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TowerDefense.Simulation
{
    public readonly struct ReplayEntry
    {
        public readonly long Tick;
        public readonly Command Command;

        public ReplayEntry(long tick, Command command)
        {
            Tick = tick;
            Command = command;
        }
    }

    /// <summary>
    /// A full run in a few kilobytes: balance version + seed + every (tick, command). Re-simulating it must reproduce
    /// the same final state hash; this powers ghosts, duels and server-side score checks (D19).
    /// Text format R2: <c>R2|game|balance|mode|core|grade|seed|hash|waves|damage|ticks|tick,type,a,b;...</c>.
    /// R1 (<c>R1|balance|seed|hash|waves|damage|ticks|commands</c>) is still read as Standard Core, Grade 0, Run.
    /// </summary>
    public sealed class Replay
    {
        public const string FormatTag = "R2";
        private const string LegacyTag = "R1";

        public string GameVersion { get; set; } = RunConfig.GameVersion;
        public string BalanceVersion { get; set; }
        public RunMode Mode { get; set; }
        public CoreType Core { get; set; }
        public int Grade { get; set; }
        public ulong Seed { get; set; }
        public ulong FinalHash { get; set; }
        public int WavesCleared { get; set; }
        public long TotalDamage { get; set; }
        public long Ticks { get; set; }
        public List<ReplayEntry> Entries { get; } = new List<ReplayEntry>();

        public static Replay Record(GameSimulation simulation)
        {
            var replay = new Replay
            {
                GameVersion = RunConfig.GameVersion,
                BalanceVersion = RunConfig.BalanceVersion,
                Mode = simulation.Config.Mode,
                Core = simulation.Config.Core,
                Grade = simulation.Config.Grade,
                Seed = simulation.Config.Seed,
                FinalHash = simulation.ComputeStateHash(),
                WavesCleared = simulation.WavesCleared,
                TotalDamage = simulation.TotalDamage,
                Ticks = simulation.Tick,
            };

            foreach ((long tick, Command command) in simulation.CommandLog)
            {
                replay.Entries.Add(new ReplayEntry(tick, command));
            }

            return replay;
        }

        public string Serialize()
        {
            var builder = new StringBuilder();
            builder.Append(FormatTag).Append('|')
                .Append(GameVersion).Append('|')
                .Append(BalanceVersion).Append('|')
                .Append((int)Mode).Append('|')
                .Append((int)Core).Append('|')
                .Append(Grade).Append('|')
                .Append(Seed.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(FinalHash.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(WavesCleared.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(TotalDamage.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(Ticks.ToString(CultureInfo.InvariantCulture)).Append('|');

            for (int i = 0; i < Entries.Count; i++)
            {
                ReplayEntry entry = Entries[i];
                if (i > 0)
                {
                    builder.Append(';');
                }

                builder.Append(entry.Tick.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append((int)entry.Command.Type).Append(',')
                    .Append(entry.Command.A.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(entry.Command.B.ToString(CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        public static Replay Deserialize(string text)
        {
            string[] parts = text.Split('|');
            Replay replay;
            string commands;
            if (parts.Length == 12 && parts[0] == FormatTag)
            {
                replay = new Replay
                {
                    GameVersion = parts[1],
                    BalanceVersion = parts[2],
                    Mode = (RunMode)int.Parse(parts[3], CultureInfo.InvariantCulture),
                    Core = (CoreType)int.Parse(parts[4], CultureInfo.InvariantCulture),
                    Grade = int.Parse(parts[5], CultureInfo.InvariantCulture),
                    Seed = ulong.Parse(parts[6], CultureInfo.InvariantCulture),
                    FinalHash = ulong.Parse(parts[7], CultureInfo.InvariantCulture),
                    WavesCleared = int.Parse(parts[8], CultureInfo.InvariantCulture),
                    TotalDamage = long.Parse(parts[9], CultureInfo.InvariantCulture),
                    Ticks = long.Parse(parts[10], CultureInfo.InvariantCulture),
                };
                commands = parts[11];
            }
            else if (parts.Length == 8 && parts[0] == LegacyTag)
            {
                replay = new Replay
                {
                    GameVersion = "unknown",
                    BalanceVersion = parts[1],
                    Seed = ulong.Parse(parts[2], CultureInfo.InvariantCulture),
                    FinalHash = ulong.Parse(parts[3], CultureInfo.InvariantCulture),
                    WavesCleared = int.Parse(parts[4], CultureInfo.InvariantCulture),
                    TotalDamage = long.Parse(parts[5], CultureInfo.InvariantCulture),
                    Ticks = long.Parse(parts[6], CultureInfo.InvariantCulture),
                };
                commands = parts[7];
            }
            else
            {
                throw new FormatException("Unknown replay format.");
            }

            if (commands.Length == 0)
            {
                return replay;
            }

            foreach (string item in commands.Split(';'))
            {
                string[] fields = item.Split(',');
                var command = new Command(
                    (CommandType)int.Parse(fields[1], CultureInfo.InvariantCulture),
                    int.Parse(fields[2], CultureInfo.InvariantCulture),
                    int.Parse(fields[3], CultureInfo.InvariantCulture));
                replay.Entries.Add(new ReplayEntry(long.Parse(fields[0], CultureInfo.InvariantCulture), command));
            }

            return replay;
        }
    }

    public readonly struct ReplayCheck
    {
        public readonly bool IsValid;
        public readonly ulong ExpectedHash;
        public readonly ulong ActualHash;
        public readonly string Reason;

        public ReplayCheck(bool isValid, ulong expectedHash, ulong actualHash, string reason)
        {
            IsValid = isValid;
            ExpectedHash = expectedHash;
            ActualHash = actualHash;
            Reason = reason;
        }
    }

    /// <summary>Re-simulates a replay from scratch and compares the final state (anti-cheat, ghosts).</summary>
    public static class ReplayVerifier
    {
        /// <summary>Safety net: one hour of wave time at 60 ticks per second.</summary>
        private const long MaxTicks = 60L * 60 * SimConstants.TicksPerSecond;

        public static ReplayCheck Verify(Replay replay, ContentDatabase content, Func<RunConfig> configFactory)
        {
            if (replay.BalanceVersion != RunConfig.BalanceVersion)
            {
                return new ReplayCheck(false, replay.FinalHash, 0, $"Balance version {replay.BalanceVersion} != {RunConfig.BalanceVersion}");
            }

            RunConfig config = configFactory();
            config.Seed = replay.Seed;
            RunSetup.Configure(config, replay.Core, replay.Grade, replay.Mode);
            GameSimulation simulation = Resimulate(replay, new GameSimulation(config, content));
            ulong actual = simulation.ComputeStateHash();
            bool valid = actual == replay.FinalHash
                && simulation.WavesCleared == replay.WavesCleared
                && simulation.TotalDamage == replay.TotalDamage;
            return new ReplayCheck(valid, replay.FinalHash, actual, valid ? "OK" : "State mismatch");
        }

        /// <summary>Feeds every command at its tick. Commands stamped with the same tick are applied in order.</summary>
        public static GameSimulation Resimulate(Replay replay, GameSimulation simulation)
        {
            int index = 0;
            List<ReplayEntry> entries = replay.Entries;
            while (simulation.Tick <= MaxTicks)
            {
                while (index < entries.Count && entries[index].Tick == simulation.Tick)
                {
                    simulation.Enqueue(entries[index++].Command);
                }

                if (simulation.IsOver || simulation.Tick >= replay.Ticks && index >= entries.Count)
                {
                    simulation.ApplyPendingCommandsNow();
                    break;
                }

                if (simulation.Phase == GamePhase.Shop)
                {
                    simulation.ApplyPendingCommandsNow();
                    bool moreCommandsNow = index < entries.Count && entries[index].Tick == simulation.Tick;
                    if (simulation.Phase == GamePhase.Shop && !moreCommandsNow)
                    {
                        // The run ended in the shop (player quit): nothing else can happen.
                        break;
                    }

                    continue;
                }

                simulation.Step();
            }

            return simulation;
        }
    }
}
