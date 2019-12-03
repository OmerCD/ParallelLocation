using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MessageObjectRouter
{
    public static class KeyParseRouterExtensions
    {
        public static IServiceCollection AddRouteParser<T, TKey, TKeyType>(this IServiceCollection services,
            Action<ParseRouterBuilder<T, TKey, TKeyType>> action) where T : IKeyParseRouter<TKey,TKeyType>, new()
        {
            var builder = new ParseRouterBuilder<T, TKey, TKeyType>();
            action(builder);
            services.AddSingleton<IParseRouter<TKeyType>>(builder.Build());
            return services;
        }
    }
    public class ParseRouterBuilder<T, TKey, TKeyType> where T : IKeyParseRouter<TKey,TKeyType>, new()
    {
        private readonly IKeyParseRouter<TKey,TKeyType> _parseRouter;

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
                _parseRouter.Types.Add(key ,type);
            }
            else
            {
                throw new Exception($"Key already exists");
            }

            return this;
        }

        public IKeyParseRouter<TKey, TKeyType> Build()
        {
            return _parseRouter;
        }
    }

    public class ByteParseRouter<TKey> : IKeyParseRouter<TKey, byte[]>
    {
        public IKeyExtractor<TKey, byte[]> KeyExtractor { get; set; }
        public IDictionary<TKey, Type> Types { get; }

        public ByteParseRouter(IKeyExtractor<TKey, byte[]> keyExtractor, IDictionary<TKey, Type> types)
        {
            KeyExtractor = keyExtractor;
            Types = types;
        }

        public ByteParseRouter()
        {
            Types = new Dictionary<TKey, Type>();
        }
        public object GetObject(byte[] bytes)
        {
            TKey key = KeyExtractor.Extract(bytes);
            Type type = Types[key];
            return Activator.CreateInstance(type);
        }
    }
}