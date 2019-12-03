using System;

namespace ReflectorO.Exceptions
{
    public class ElectorDuplicateTypeException : Exception
    {
        public ElectorDuplicateTypeException(Type type) : base($"Type {type.Name} already exists")
        {
            
        }
    }
}