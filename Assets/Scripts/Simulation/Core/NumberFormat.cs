using System.Globalization;

namespace TowerDefense.Simulation
{
    /// <summary>
    /// Compact notation for "numbers that explode" (GDD v0.2 §7): plain digits up to 9,999, then 12.3K · 4.5M · 6.7B · 8.9T.
    /// Integer arithmetic only, invariant culture, so the same value prints the same everywhere (replays, ghosts, tests).
    /// </summary>
    public static class NumberFormat
    {
        private const long CompactFrom = 10_000;
        private static readonly string[] Suffixes = { "K", "M", "B", "T" };

        /// <summary>Formats a value stored in hundredths (the simulation's HP/damage unit).</summary>
        public static string CompactHundredths(long hundredths) => Compact(hundredths / SimConstants.HpScale);

        /// <summary>Formats a whole number. Negative values keep their sign.</summary>
        public static string Compact(long value)
        {
            if (value < 0)
            {
                return "-" + Compact(-value);
            }

            if (value < CompactFrom)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            int suffix = -1;
            long scaled = value;
            while (scaled >= 1000 && suffix < Suffixes.Length - 1)
            {
                scaled /= 1000;
                suffix++;
            }

            // One decimal while the leading part has fewer than three digits (12.3K), none afterwards (123K).
            long divisor = 1;
            for (int i = 0; i <= suffix; i++)
            {
                divisor *= 1000;
            }

            long whole = value / divisor;
            long tenth = (value % divisor) * 10 / divisor;
            string text = whole < 100 && tenth > 0
                ? whole.ToString(CultureInfo.InvariantCulture) + "." + tenth.ToString(CultureInfo.InvariantCulture)
                : whole.ToString(CultureInfo.InvariantCulture);
            return text + Suffixes[suffix];
        }
    }
}
