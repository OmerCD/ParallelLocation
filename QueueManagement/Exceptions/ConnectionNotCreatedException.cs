using System;

namespace Parallel.Main.Exceptions
{
    public class ConnectionNotCreatedException : Exception
    {
        public ConnectionNotCreatedException() : base("First you must start the connection to the Queue")
        {
            
        }

        public ConnectionNotCreatedException(string message) : base(message)
        {
            
        }
    }
}