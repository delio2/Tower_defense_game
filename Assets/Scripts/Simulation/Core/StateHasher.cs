namespace TowerDefense.Simulation
{
    /// <summary>
    /// FNV-1a 64-bit hasher used to compare simulation states (determinism tests, replay checks).
    /// </summary>
    public struct StateHasher
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        private ulong _hash;
        private bool _initialized;

        public ulong Value => _initialized ? _hash : OffsetBasis;

        public void Add(long value)
        {
            if (!_initialized)
            {
                _hash = OffsetBasis;
                _initialized = true;
            }

            unchecked
            {
                ulong bits = (ulong)value;
                for (int i = 0; i < 8; i++)
                {
                    _hash ^= bits & 0xFF;
                    _hash *= Prime;
                    bits >>= 8;
                }
            }
        }
    }
}
