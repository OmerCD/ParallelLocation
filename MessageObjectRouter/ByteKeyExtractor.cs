using System.Collections.Generic;

namespace MessageObjectRouter
{
    public class ByteKeyExtractor : IKeyExtractor<byte[], byte[]>
    {
        private readonly IDictionary<byte, int> _lengths;

        public ByteKeyExtractor(IDictionary<byte, int> keys)
        {
            _lengths = keys;
        }

        public ByteKeyExtractor()
        {
            _lengths = new Dictionary<byte, int>();
        }

        public byte[] Extract(byte[] item)
        {
            if (_lengths.TryGetValue(item[0], out int length))
            {
                return item[..length];
            }
            else
            {
                throw new KeyNotFoundException($"Given key {item[0]} was not found.");
            }
        }
    }
}