namespace PasswordGen.Utilities
{
    using System.Diagnostics;
    using System.Security.Cryptography;

    internal static class CryptoRNG
    {
        private const int BYTE_RANGE = 256;
        private const int DATA_SIZE = 4096;

        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static readonly byte[] _data = new byte[DATA_SIZE];
        private static int _index = -1;

        static CryptoRNG() => _rng.GetBytes(_data);

        /// <summary>
        /// Return a random integer in the byte range and less than <paramref name="limit"/>.
        /// </summary>
        internal static int GetRandomInt(int limit)
        {
            Debug.Assert(limit > 0 && limit <= BYTE_RANGE);

            // Values above max are skipped to avoid a modulo bias
            int max = byte.MaxValue - (BYTE_RANGE % limit);

            do
            {
                if (++_index == DATA_SIZE)
                {
                    _rng.GetBytes(_data);
                    _index = 0;
                }
            } while (_data[_index] > max);
            return _data[_index] % limit;
        }
    }
}
