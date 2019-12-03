using System;
using System.Collections.Generic;

namespace MessageObjectRouter
{
    public interface IParseRouter<in T>
    {
        object GetObject(T bytes);
    }

    public interface IKeyParseRouter<TKey, T> : IParseRouter<T>
    {
        IKeyExtractor<TKey, T> KeyExtractor { get; set; }
        IDictionary<TKey, Type> Types { get; }
    }
}