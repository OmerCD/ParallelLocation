using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using ReflectorO;

namespace MessageObjectRouter
{
    public static class KeyParseRouterExtensions
    {
        public static IServiceCollection AddRouteParser<T, TKey, TKeyType>(this IServiceCollection services,
            Action<ParseRouterBuilder<T, TKey, TKeyType>> action) where T : IKeyParseRouter<TKey, TKeyType>, new()
        {
            var builder = new ParseRouterBuilder<T, TKey, TKeyType>();
            action(builder);
            services.AddSingleton<IParseRouter<TKeyType>>(builder.Build());
            return services;
        }
    }

    public class ParseRouterBuilder<T, TKey, TKeyType> where T : IKeyParseRouter<TKey, TKeyType>, new()
    {
        private readonly IKeyParseRouter<TKey, TKeyType> _parseRouter;

        public ParseRouterBuilder()
        {
            _parseRouter = new T();
        }

        public ParseRouterBuilder<T, TKey, TKeyType> UseKeyExtractor<TExtractor>(TExtractor extractor)
            where TExtractor : IKeyExtractor<TKey, TKeyType>
        {
            _parseRouter.KeyExtractor = extractor;
            return this;
        }

        public ParseRouterBuilder<T, TKey, TKeyType> AddType(TKey key, Type type)
        {
            if (!_parseRouter.Types.ContainsKey(key))
            {
                _parseRouter.AddType(key, type);
            }
            else
            {
                throw new Exception($"Key already exists");
            }

            return this;
        }

        public ParseRouterBuilder<T, TKey, TKeyType> UseComparer(IEqualityComparer<TKey> comparer)
        {
            _parseRouter.EqualityComparer = comparer;
            _parseRouter.Types = new Dictionary<TKey, Type>(_parseRouter.Types, comparer);

            return this;
        }

        public IKeyParseRouter<TKey, TKeyType> Build()
        {
            return _parseRouter;
        }
    }

    public class ByteParseRouter<TKey> : IKeyParseRouter<TKey, byte[]>
    {
        private readonly IElector _elector;
        public IKeyExtractor<TKey, byte[]> KeyExtractor { get; set; }
        public IDictionary<TKey, Type> Types { get; set; }
        public IEqualityComparer<TKey> EqualityComparer { get; set; }

        public void AddType(TKey key, Type type)
        {
            Types.Add(key, type);
            _elector.RegisterType(type);
        }

        public ByteParseRouter(IKeyExtractor<TKey, byte[]> keyExtractor, IDictionary<TKey, Type> types,
            IEqualityComparer<TKey> equalityComparer)
        {
            KeyExtractor = keyExtractor;
            Types = types;
            EqualityComparer = equalityComparer;
        }

        public ByteParseRouter()
        {
            Types = new Dictionary<TKey, Type>();
            _elector = new Elector(EndianType.BigEndian);
        }

        public object GetObject(byte[] bytes)
        {
            TKey key = KeyExtractor.Extract(bytes);
            if (key != null && Types.TryGetValue(key, out var foundType) && foundType != null)
            {
                return _elector.CreateObject(bytes, foundType);

            }

            return null;
        }

        public Type GetType(byte[] item)
        {
            TKey key = KeyExtractor.Extract(item);
            Type type = Types[key];
            return type;
        }
    }
}