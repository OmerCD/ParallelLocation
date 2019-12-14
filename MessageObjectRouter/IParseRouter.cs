using System;
using System.Collections.Generic;

namespace MessageObjectRouter
{
    public interface IParseRouter<in T>
    {
        object GetObject(T bytes);
        Type GetType(T item);
    }

    public interface IKeyParseRouter<TKey, T> : IParseRouter<T>
    {
        IKeyExtractor<TKey, T> KeyExtractor { get; set; }
        IDictionary<TKey, Type> Types { get; set; }
        IEqualityComparer<TKey> EqualityComparer { get; set; }
        void AddType(TKey key, Type type);
    }
}