namespace TowerDefense.Simulation
{
    /// <summary>
    /// Deterministic PCG32 random number generator (O'Neill). Identical results on every platform.
    /// </summary>
    public sealed class Pcg32
    {
        private const ulong Multiplier = 6364136223846793005UL;

        private ulong _state;
        private readonly ulong _increment;

        public Pcg32(ulong seed, ulong streamId)
        {
            _increment = (streamId << 1) | 1UL;
            _state = 0UL;
            NextUInt();
            _state += seed;
            NextUInt();
        }

        /// <summary>Creates an independent stream derived from a master seed and a stream id.</summary>
        public static Pcg32 ForStream(ulong masterSeed, RngStream stream)
        {
            ulong mixed = SplitMix64(masterSeed ^ ((ulong)stream * 0x9E3779B97F4A7C15UL));
            return new Pcg32(mixed, (ulong)stream);
        }

        public uint NextUInt()
        {
            ulong oldState = _state;
            _state = unchecked(oldState * Multiplier + _increment);
            uint xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            int rotation = (int)(oldState >> 59);
            return (xorShifted >> rotation) | (xorShifted << ((-rotation) & 31));
        }

        /// <summary>Returns an unbiased integer in [0, maxExclusive).</summary>
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 1)
            {
                return 0;
            }

            uint bound = (uint)maxExclusive;
            uint threshold = (uint)(-(int)bound) % bound;
            while (true)
            {
                uint value = NextUInt();
                if (value >= threshold)
                {
                    return (int)(value % bound);
                }
            }
        }

        /// <summary>Returns an integer in [minInclusive, maxExclusive).</summary>
        public int NextRange(int minInclusive, int maxExclusive)
        {
            return minInclusive + NextInt(maxExclusive - minInclusive);
        }

        public static ulong SplitMix64(ulong value)
        {
            unchecked
            {
                value += 0x9E3779B97F4A7C15UL;
                value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
                value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
                return value ^ (value >> 31);
            }
        }
    }

    /// <summary>
    /// Separate random streams so that player choices never change the map or the waves (D11).
    /// </summary>
    public enum RngStream : ulong
    {
        Waves = 1,
        Shop = 2,
        Effects = 3,
    }
}
