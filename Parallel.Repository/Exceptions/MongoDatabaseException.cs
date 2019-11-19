using System;

namespace Parallel.Repository.Exceptions
{
    public class MongoDatabaseException : Exception
    {
        public MongoDatabaseException(string message) : base(message)
        {
            
        }
    }
}