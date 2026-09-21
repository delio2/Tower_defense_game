namespace TowerDefense.Simulation
{
    /// <summary>
    /// An enemy moving radially towards the Core along one of the fixed <see cref="Directions"/>.
    /// </summary>
    public sealed class Enemy
    {
        public int Id { get; }
        public EnemyDefinition Definition { get; }
        public long MaxHp { get; }
        public long Hp { get; internal set; }
        public int Direction { get; }

        /// <summary>Distance from the Core centre, in micro-units.</summary>
        public long Radius { get; internal set; }

        public long SpeedPerTick { get; }

        /// <summary>True once the enemy has touched the Core; it is removed at the end of the tick.</summary>
        public bool ReachedCore { get; internal set; }

        /// <summary>Guardian only: how many Swarmlet summons (at 75/50/25% HP) already happened.</summary>
        internal int SummonsDone { get; set; }

        public EnemyKind Kind => Definition.Kind;
        public bool IsAlive => Hp > 0 && !ReachedCore;

        internal Enemy(int id, EnemyDefinition definition, long maxHp, int direction, long radius)
        {
            Id = id;
            Definition = definition;
            MaxHp = maxHp;
            Hp = maxHp;
            Direction = Directions.Normalize(direction);
            Radius = radius;
            SpeedPerTick = SimConstants.SpeedPerTick(definition.SpeedMilli);
        }

        public void GetPosition(out long x, out long y) => Directions.PointAt(Direction, Radius, out x, out y);
    }
}
