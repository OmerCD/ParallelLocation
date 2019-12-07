using System;

namespace ReflectorO.Exceptions
{
    public class ElectorConvertException : Exception
    {
        public ElectorConvertException(string baseTypeName, object value) : base($"Couldn't Convert {baseTypeName}{Environment.NewLine}Value : {value}")
        {

        }
    }
}