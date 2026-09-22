namespace TowerDefense.Simulation
{
    /// <summary>
    /// Fixed units used by the deterministic simulation. No floating point is used for game state (D11).
    /// </summary>
    public static class SimConstants
    {
        public const int TicksPerSecond = 60;

        /// <summary>Positions and distances are stored in micro-units: one world unit is this many micro-units.</summary>
        public const long Micro = 1_000_000;

        /// <summary>Balance data expresses distances in milli-units (1000 = one world unit).</summary>
        public const long MilliToMicro = 1_000;

        /// <summary>Hit points and damage are stored in hundredths (2000 = 20.00 HP).</summary>
        public const long HpScale = 100;

        /// <summary>Common scale for ratios and multipliers expressed in thousandths.</summary>
        public const int Permille = 1000;

        public const long CoreRadius = 700 * MilliToMicro;
        public const long RingRadius = 1500 * MilliToMicro;
        public const long SpawnRadius = 9000 * MilliToMicro;

        /// <summary>Converts a speed in milli-units per second into micro-units per tick.</summary>
        public static long SpeedPerTick(int milliPerSecond) => milliPerSecond * MilliToMicro / TicksPerSecond;

        public static long MilliToMicroUnits(int milli) => milli * MilliToMicro;
    }

    /// <summary>
    /// 192 fixed directions (1.875 degrees apart) with integer cosine/sine scaled by 10 000. The table is data, so
    /// every platform computes exactly the same positions. 192 is divisible by 6 and 8, the possible ring sizes.
    /// </summary>
    public static class Directions
    {
        public const int Count = 192;
        public const int Scale = 10_000;

        /// <summary>Direction pointing up (+Y); ring slot 0 sits here.</summary>
        public const int Up = Count / 4;

        private static readonly int[] CosTable =
        {
            10000, 9995, 9979, 9952, 9914, 9866, 9808, 9739, 9659, 9569, 9469, 9359, 9239, 9109, 8969, 8819,
            8660, 8492, 8315, 8128, 7934, 7730, 7518, 7299, 7071, 6836, 6593, 6344, 6088, 5825, 5556, 5281,
            5000, 4714, 4423, 4127, 3827, 3523, 3214, 2903, 2588, 2271, 1951, 1629, 1305, 980, 654, 327,
            0, -327, -654, -980, -1305, -1629, -1951, -2271, -2588, -2903, -3214, -3523, -3827, -4127, -4423, -4714,
            -5000, -5281, -5556, -5825, -6088, -6344, -6593, -6836, -7071, -7299, -7518, -7730, -7934, -8128, -8315, -8492,
            -8660, -8819, -8969, -9109, -9239, -9359, -9469, -9569, -9659, -9739, -9808, -9866, -9914, -9952, -9979, -9995,
            -10000, -9995, -9979, -9952, -9914, -9866, -9808, -9739, -9659, -9569, -9469, -9359, -9239, -9109, -8969, -8819,
            -8660, -8492, -8315, -8128, -7934, -7730, -7518, -7299, -7071, -6836, -6593, -6344, -6088, -5825, -5556, -5281,
            -5000, -4714, -4423, -4127, -3827, -3523, -3214, -2903, -2588, -2271, -1951, -1629, -1305, -980, -654, -327,
            0, 327, 654, 980, 1305, 1629, 1951, 2271, 2588, 2903, 3214, 3523, 3827, 4127, 4423, 4714,
            5000, 5281, 5556, 5825, 6088, 6344, 6593, 6836, 7071, 7299, 7518, 7730, 7934, 8128, 8315, 8492,
            8660, 8819, 8969, 9109, 9239, 9359, 9469, 9569, 9659, 9739, 9808, 9866, 9914, 9952, 9979, 9995,
        };

        public static int Normalize(int direction) => ((direction % Count) + Count) % Count;

        public static int Cos(int direction) => CosTable[Normalize(direction)];

        /// <summary>sin(a) = cos(a - 90 degrees).</summary>
        public static int Sin(int direction) => CosTable[Normalize(direction - Up)];

        /// <summary>Point at <paramref name="radius"/> micro-units along <paramref name="direction"/>.</summary>
        public static void PointAt(int direction, long radius, out long x, out long y)
        {
            x = radius * Cos(direction) / Scale;
            y = radius * Sin(direction) / Scale;
        }

        /// <summary>
        /// Direction of ring slot <paramref name="slot"/> out of <paramref name="slotCount"/>, clockwise from the top,
        /// rounded to the nearest table entry (7 slots do not divide 192 evenly; the error is under one step).
        /// </summary>
        public static int OfSlot(int slot, int slotCount) => Normalize(Up - (slot * Count + slotCount / 2) / slotCount);
    }
}
