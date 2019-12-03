using System;

namespace ReflectorO
{
    public interface IElector
    {
        byte[] CreateByteArray(object @object);
        object CreateObject(byte[] bytes, Type type);
        void RegisterType(Type type);
    }
}