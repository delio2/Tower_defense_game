namespace TowerDefense.Simulation
{
    /// <summary>
    /// A module mounted on a ring slot. Effective stats are recomputed by <see cref="Ring.Recalculate"/>
    /// whenever the ring changes (only possible in the shop).
    /// </summary>
    public sealed class ModuleInstance
    {
        public int Id { get; }
        public ModuleDefinition Definition { get; }
        public int Level { get; internal set; } = 1;
        public int Invested { get; internal set; }
        public int Slot { get; internal set; }

        /// <summary>Ticks left before the next shot (weapons only).</summary>
        public int CooldownRemaining { get; internal set; }

        /// <summary>Damage per hit after level, neighbour boosters and global multipliers, in hundredths.</summary>
        public long EffectiveDamage { get; internal set; }

        public int EffectiveCooldown { get; internal set; }

        /// <summary>Range in micro-units, measured from the module position on the ring.</summary>
        public long EffectiveRange { get; internal set; }

        /// <summary>Combined damage multiplier from neighbours, permille (for UI: "x2.25").</summary>
        public long DamageMultiplierPermille { get; internal set; } = SimConstants.Permille;

        /// <summary>Echo from neighbours: every hit fires a second one at this fraction (0 = none), permille.</summary>
        public int EchoPermille { get; internal set; }

        public ModuleKind Kind => Definition.Kind;
        public ModuleCategory Category => Definition.Category;

        internal ModuleInstance(int id, ModuleDefinition definition, int slot, int invested)
        {
            Id = id;
            Definition = definition;
            Slot = slot;
            Invested = invested;
        }
    }
}
