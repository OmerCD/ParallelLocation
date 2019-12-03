using System;
using System.Collections.Generic;

namespace MessageObjectRouter
{
    public class ByteParseRouter<TKey> : IParseRouter<byte[]>
    {
        private readonly IDictionary<TKey, Type> _types;
        private readonly IKeyExtractor<TKey, byte[]> _keyExtractor;

        public ByteParseRouter(IKeyExtractor<TKey, byte[]> keyExtractor, IDictionary<TKey, Type> types)
        {
            _keyExtractor = keyExtractor;
            _types = types;
        }

        public object GetObject(byte[] bytes)
        {
            TKey key = _keyExtractor.Extract(bytes);
            Type type = _types[key];
            return Activator.CreateInstance(type);
        }
    }
}