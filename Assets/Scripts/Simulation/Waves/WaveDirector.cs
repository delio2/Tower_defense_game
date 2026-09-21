using System.Collections.Generic;

namespace TowerDefense.Simulation
{
    public readonly struct SpawnEntry
    {
        public readonly EnemyKind Kind;

        /// <summary>Ticks after the wave start.</summary>
        public readonly int OffsetTicks;

        public readonly int Direction;

        public SpawnEntry(EnemyKind kind, int offsetTicks, int direction)
        {
            Kind = kind;
            OffsetTicks = offsetTicks;
            Direction = direction;
        }
    }

    /// <summary>
    /// Builds waves from the seeded wave stream (GDD v0.2 §9). Rules: a newly unlocked enemy enters alone first,
    /// max 3 types per wave, wave 3 of each act is themed, wave 6 is the Guardian.
    /// </summary>
    public sealed class WaveDirector
    {
        private const int IntroductionGapTicks = 2 * SimConstants.TicksPerSecond;
        private const int GuardianEscortDelayTicks = 3 * SimConstants.TicksPerSecond;
        private const int ThemedWave = 3;
        private const int MaxTypesPerWave = 3;
        private const int GroupDirectionSpread = 2;

        /// <summary>Wave (within the act) in which each enemy type becomes available. Prototype roster.</summary>
        private static readonly (EnemyKind Kind, int FromWave)[] Unlocks =
        {
            (EnemyKind.Drifter, 1),
            (EnemyKind.Swarmlet, 2),
            (EnemyKind.Brute, 3),
        };

        private readonly RunConfig _config;
        private readonly ContentDatabase _content;
        private readonly Pcg32 _rng;

        public WaveDirector(RunConfig config, ContentDatabase content, Pcg32 waveRng)
        {
            _config = config;
            _content = content;
            _rng = waveRng;
        }

        public int WaveInAct(int globalWave) => (globalWave - 1) % _config.WavesPerAct + 1;

        public bool IsGuardianWave(int globalWave) => WaveInAct(globalWave) == _config.WavesPerAct;

        /// <summary>HP multiplier for a global wave (1-based), in parts per million. Iterative: no Math.Pow (D11).</summary>
        public long HpMultiplierPpm(int globalWave)
        {
            long value = 1_000_000;
            for (int i = 1; i < globalWave; i++)
            {
                value = value * _config.HpGrowthPermille / SimConstants.Permille;
            }

            return value;
        }

        /// <summary>Budget for a global wave (1-based), in thousandths of a point.</summary>
        public long BudgetMilli(int globalWave)
        {
            long value = _config.FirstWaveBudgetMilli;
            for (int i = 1; i < globalWave; i++)
            {
                value = value * _config.BudgetGrowthPermille / SimConstants.Permille;
            }

            return value;
        }

        public long EnemyMaxHp(EnemyDefinition definition, int globalWave)
        {
            long hp = definition.Hp * HpMultiplierPpm(globalWave) / 1_000_000;
            return hp < SimConstants.HpScale ? SimConstants.HpScale : hp;
        }

        /// <summary>Generates the spawn list of a wave. Must be called once per wave, in order.</summary>
        public List<SpawnEntry> BuildWave(int globalWave)
        {
            int waveInAct = WaveInAct(globalWave);
            bool guardian = IsGuardianWave(globalWave);

            var available = new List<EnemyKind>();
            EnemyKind? introduced = null;
            foreach (var (kind, fromWave) in Unlocks)
            {
                if (waveInAct >= fromWave)
                {
                    available.Add(kind);
                }

                if (waveInAct == fromWave && fromWave > 1)
                {
                    introduced = kind;
                }
            }

            var chosen = new List<EnemyKind>();
            if (waveInAct == ThemedWave && !guardian)
            {
                chosen.Add(available[_rng.NextInt(available.Count)]);
            }
            else
            {
                var pool = new List<EnemyKind>(available);
                while (chosen.Count < MaxTypesPerWave && pool.Count > 0)
                {
                    int index = _rng.NextInt(pool.Count);
                    chosen.Add(pool[index]);
                    pool.RemoveAt(index);
                }
            }

            var groups = new List<EnemyKind>();
            long budget = BudgetMilli(globalWave);
            int startOffset = 0;
            var spawns = new List<SpawnEntry>();

            if (guardian)
            {
                spawns.Add(new SpawnEntry(EnemyKind.Guardian, 0, _rng.NextInt(Directions.Count)));
                budget /= 2;
                startOffset = GuardianEscortDelayTicks;
            }
            else if (introduced.HasValue)
            {
                // The new enemy enters alone first so that it can be read (GDD v0.2 §9).
                AddGroup(spawns, introduced.Value, 0, _rng.NextInt(Directions.Count));
                budget -= GroupCost(introduced.Value);
                startOffset = IntroductionGapTicks;
            }

            var affordable = new List<EnemyKind>(chosen.Count);
            while (true)
            {
                affordable.Clear();
                foreach (EnemyKind kind in chosen)
                {
                    if (GroupCost(kind) <= budget)
                    {
                        affordable.Add(kind);
                    }
                }

                if (affordable.Count == 0)
                {
                    break;
                }

                EnemyKind pick = affordable[_rng.NextInt(affordable.Count)];
                groups.Add(pick);
                budget -= GroupCost(pick);
            }

            int window = _config.SpawnWindowTicks - startOffset;
            int interval = groups.Count > 0 ? window / groups.Count : 0;
            for (int i = 0; i < groups.Count; i++)
            {
                AddGroup(spawns, groups[i], startOffset + i * interval, _rng.NextInt(Directions.Count));
            }

            return spawns;
        }

        private long GroupCost(EnemyKind kind)
        {
            EnemyDefinition definition = _content.Enemy(kind);
            return (long)definition.BudgetCostMilli * definition.GroupSize;
        }

        private void AddGroup(List<SpawnEntry> spawns, EnemyKind kind, int offset, int direction)
        {
            int size = _content.Enemy(kind).GroupSize;
            int first = -(size / 2);
            for (int i = 0; i < size; i++)
            {
                spawns.Add(new SpawnEntry(kind, offset, direction + (first + i) * GroupDirectionSpread));
            }
        }
    }
}
