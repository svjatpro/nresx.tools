using System;

namespace nresx.Tools.Exceptions
{
    public class UnknownResourceFormatException : Exception
    {
        public UnknownResourceFormatException()
        {
        }

        public UnknownResourceFormatException(string message) : base(message)
        {
        }
    }
}
