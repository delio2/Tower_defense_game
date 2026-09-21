namespace TowerDefense.Simulation
{
    public enum CommandType : byte
    {
        /// <summary>A = offer index, B = ring slot (ignored when the purchase merges).</summary>
        Buy = 0,

        /// <summary>A = ring slot.</summary>
        Sell = 1,

        /// <summary>A = from slot, B = to slot (swap).</summary>
        Move = 2,

        Reroll = 3,
        StartWave = 4,
        Pulse = 5,

        /// <summary>Reverts the last Buy/Sell/Move of the current shop visit (never a Reroll).</summary>
        Undo = 6,
    }

    /// <summary>
    /// A player action. Commands are applied at the start of a tick (or immediately in the shop, where time does not
    /// advance). The (tick, command) log reproduces a run exactly and is the replay format (D19).
    /// Pause and game speed are not commands: they never change the outcome.
    /// </summary>
    public readonly struct Command
    {
        public readonly CommandType Type;
        public readonly int A;
        public readonly int B;

        public Command(CommandType type, int a = 0, int b = 0)
        {
            Type = type;
            A = a;
            B = b;
        }

        public static Command Buy(int offerIndex, int slot) => new Command(CommandType.Buy, offerIndex, slot);
        public static Command Sell(int slot) => new Command(CommandType.Sell, slot);
        public static Command Move(int from, int to) => new Command(CommandType.Move, from, to);
        public static Command Reroll() => new Command(CommandType.Reroll);
        public static Command StartWave() => new Command(CommandType.StartWave);
        public static Command Pulse() => new Command(CommandType.Pulse);
        public static Command Undo() => new Command(CommandType.Undo);

        public override string ToString() => $"{Type}({A},{B})";
    }

    public enum CommandResult : byte
    {
        Ok = 0,
        OnlyInShop,
        OnlyDuringWave,
        InvalidOffer,
        OfferAlreadyBought,
        InvalidSlot,
        SlotOccupied,
        SlotEmpty,
        NotEnoughCredits,
        PulseNotReady,
        GameOver,
        NothingToUndo,
    }
}
