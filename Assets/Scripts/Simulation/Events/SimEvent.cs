namespace TowerDefense.Simulation
{
    public enum SimEventType : byte
    {
        WaveStarted = 0,
        WaveCleared = 1,
        ShopOpened = 2,
        EnemySpawned = 3,

        /// <summary>EntityId = enemy, Extra = module id (0 = Pulse), Value = damage dealt.</summary>
        EnemyHit = 4,

        /// <summary>EntityId = enemy, Extra = module id (0 = Pulse), Value = damage of the killing hit.</summary>
        EnemyKilled = 5,

        /// <summary>EntityId = enemy, Value = integrity lost.</summary>
        CoreHit = 6,

        PulseUsed = 7,

        /// <summary>EntityId = module, Extra = number of targets hit.</summary>
        ModuleFired = 8,

        ModuleBought = 9,
        ModuleMerged = 10,
        ModuleSold = 11,
        ModulesSwapped = 12,
        Rerolled = 13,

        /// <summary>Extra = <see cref="CommandResult"/>.</summary>
        CommandRejected = 14,

        Victory = 15,
        Defeat = 16,
        Undone = 17,

        /// <summary>Value = new slot count, Extra = index of the new slot.</summary>
        SlotAdded = 18,
    }

    /// <summary>
    /// Something that happened during a tick. Presentation reads these to drive visuals and audio;
    /// the simulation never depends on them.
    /// </summary>
    public readonly struct SimEvent
    {
        public readonly SimEventType Type;
        public readonly long Tick;
        public readonly int EntityId;
        public readonly long Value;
        public readonly int Extra;

        public SimEvent(SimEventType type, long tick, int entityId = 0, long value = 0, int extra = 0)
        {
            Type = type;
            Tick = tick;
            EntityId = entityId;
            Value = value;
            Extra = extra;
        }

        public override string ToString() => $"[{Tick}] {Type} id={EntityId} value={Value} extra={Extra}";
    }
}
