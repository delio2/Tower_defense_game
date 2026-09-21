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
