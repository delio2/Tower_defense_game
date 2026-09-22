namespace TowerDefense.Simulation
{
    public enum EnemyKind : byte
    {
        Drifter = 0,
        Swarmlet = 1,
        Brute = 2,
        Dasher = 3,
        Splitter = 4,
        Warden = 5,
        Guardian = 10,
    }

    /// <summary>
    /// Immutable balance data for one enemy type (GDD v0.2 §8).
    /// </summary>
    public sealed class EnemyDefinition
    {
        public EnemyKind Kind { get; }

        /// <summary>Hit points at wave 1, in hundredths.</summary>
        public long Hp { get; }

        /// <summary>Flat damage reduction per hit, in hundredths.</summary>
        public long Armor { get; }

        public int SpeedMilli { get; }

        /// <summary>Core integrity removed on contact, in hundredths.</summary>
        public long ContactDamage { get; }

        /// <summary>Wave budget cost in thousandths of a point.</summary>
        public int BudgetCostMilli { get; }

        /// <summary>How many enemies spawn together (Swarmlet groups).</summary>
        public int GroupSize { get; }

        /// <summary>Dasher: every <see cref="DashEveryTicks"/> ticks it moves at <see cref="DashSpeedPermille"/> for <see cref="DashTicks"/> ticks.</summary>
        public int DashEveryTicks { get; set; }
        public int DashTicks { get; set; }
        public int DashSpeedPermille { get; set; } = SimConstants.Permille;

        /// <summary>Splitter: on death spawns this many enemies of this kind where it died.</summary>
        public EnemyKind SplitInto { get; set; }
        public int SplitCount { get; set; }

        /// <summary>Warden: enemies within the radius take this fraction of damage (500 = −50%).</summary>
        public int ShieldRadiusMilli { get; set; }
        public int ShieldPermille { get; set; } = SimConstants.Permille;

        /// <summary>Guardian: number of enemies summoned every 25% of health lost.</summary>
        public int SummonCount { get; set; }
        public EnemyKind SummonKind { get; set; }

        public bool IsGuardian => Kind == EnemyKind.Guardian;

        public EnemyDefinition(EnemyKind kind, long hp, long armor, int speedMilli, long contactDamage, int budgetCostMilli, int groupSize = 1)
        {
            Kind = kind;
            Hp = hp;
            Armor = armor;
            SpeedMilli = speedMilli;
            ContactDamage = contactDamage;
            BudgetCostMilli = budgetCostMilli;
            GroupSize = groupSize;
        }
    }
}
