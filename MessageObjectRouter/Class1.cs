using System;
using System.Collections.Generic;

namespace MessageObjectRouter
{
    public class Class1
    {
    }

    public class ParseRouter
    {
        private Dictionary<byte, byte> _checkCount;
        private ITypeRouter _typeRouter;
        public ParseRouter(IEnumerable<KeyValuePair<byte,byte>> checkCount, ITypeRouter typeRouter)
        {
            _typeRouter = typeRouter;
            _checkCount = new Dictionary<byte, byte>(checkCount);
        }

        public object Route(byte[] objectArray)
        {
            Type type = _typeRouter.GetRouteType(objectArray);
            return Activator.CreateInstance(type);
        }
    }

    interface ITypeCheckFilter<T>
    {
        T FilterRoutedObject(Func<T, T> filter);
    }
    internal interface ITypeRouter<T>
    {
        Type GetRouteType(T checkObject);
    }
    public class ByteTypeRouter : ITypeRouter<byte[]>
    {
        public Type GetRouteType(byte[] checkObject)
        {
            
            
        }
    }
}