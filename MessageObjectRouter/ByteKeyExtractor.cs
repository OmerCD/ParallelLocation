using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MessageObjectRouter
{
    public class KeyExtractorBuilder<TRef, T, TKey, TValue> where T : IKeyHolder<TKey, TValue>, TRef, new()
    {
        private readonly T _keyHolder;

        public KeyExtractorBuilder()
        {
            _keyHolder = new T();
        }

        public KeyExtractorBuilder<TRef, T, TKey, TValue> AddKeyInfo(TKey key, TValue value)
        {
            _keyHolder.AddKey(key, value);
            return this;
        }

        public T Build()
        {
            return _keyHolder;
        }
    }

    public static class KeyExtractorExtensions
    {
        public static IServiceCollection AddKeyExtractor<TRef, T, TKey, TValue>(this IServiceCollection services,
            Action<KeyExtractorBuilder<TRef, T, TKey, TValue>> buildAction)
            where T : class, IKeyHolder<TKey, TValue>, TRef, new()
            where TRef : class
        {
            var builder = new KeyExtractorBuilder<TRef, T, TKey, TValue>();
            buildAction(builder);
            services.AddSingleton<TRef>(builder.Build());
            return services;
        }
    }

    public class ByteKeyExtractor : IKeyExtractor<byte[], byte[]>, IKeyHolder<byte, int>
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

        public IKeyHolder<byte, int> AddKey(byte key, int value)
        {
            if (!_lengths.ContainsKey(key))
            {
                _lengths.Add(key, value);
            }
            else
            {
                throw new Exception($"Key already exists : {key}");
            }

            return this;
        }
    }

    public interface IKeyHolder<in TKey, in TValue>
    {
        IKeyHolder<TKey, TValue> AddKey(TKey key, TValue value);
    }
}