using System;

namespace Fastql.Exceptions
{
    public class FastqlException : Exception
    {
        public FastqlException(string message) : base(message) { }
        public FastqlException(string message, Exception innerException) : base(message, innerException) { }
    }
}
