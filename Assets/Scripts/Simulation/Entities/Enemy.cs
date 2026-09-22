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

        /// <summary>Flat damage reduction per hit, in hundredths (elites get +1 over the definition).</summary>
        public long Armor { get; }

        /// <summary>Elite (GDD v0.2 §8): HP ×3, armor +1, double halo in the presentation.</summary>
        public bool IsElite { get; }

        /// <summary>Distance from the Core centre, in micro-units.</summary>
        public long Radius { get; internal set; }

        /// <summary>Base speed per tick; dashes and slows are applied on top (see <see cref="GameSimulation"/>).</summary>
        public long SpeedPerTick { get; }

        /// <summary>True once the enemy has touched the Core; it is removed at the end of the tick.</summary>
        public bool ReachedCore { get; internal set; }

        /// <summary>Guardian only: how many summons (at 75/50/25% HP) already happened.</summary>
        internal int SummonsDone { get; set; }

        /// <summary>Dasher only: ticks alive, to schedule dashes.</summary>
        internal int AgeTicks { get; set; }

        public EnemyKind Kind => Definition.Kind;
        public bool IsAlive => Hp > 0 && !ReachedCore;

        /// <summary>Dasher: true during the last <see cref="EnemyDefinition.DashTicks"/> of every <see cref="EnemyDefinition.DashEveryTicks"/> cycle.</summary>
        public bool IsDashing => Definition.DashEveryTicks > 0
            && AgeTicks % Definition.DashEveryTicks >= Definition.DashEveryTicks - Definition.DashTicks;

        internal Enemy(int id, EnemyDefinition definition, long maxHp, int direction, long radius, bool isElite = false)
        {
            Id = id;
            Definition = definition;
            IsElite = isElite;
            MaxHp = isElite ? maxHp * 3 : maxHp;
            Hp = MaxHp;
            Armor = definition.Armor + (isElite ? SimConstants.HpScale : 0);
            Direction = Directions.Normalize(direction);
            Radius = radius;
            SpeedPerTick = SimConstants.SpeedPerTick(definition.SpeedMilli);
        }

        public void GetPosition(out long x, out long y) => Directions.PointAt(Direction, Radius, out x, out y);
    }
}
