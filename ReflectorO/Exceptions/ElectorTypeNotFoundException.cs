using System;

namespace ReflectorO.Exceptions
{
    public class ElectorTypeNotFoundException:Exception
    {
        public ElectorTypeNotFoundException(Type type) : base($"The type {type.Name} was not added to be used")
        {
            
        }
    }
}