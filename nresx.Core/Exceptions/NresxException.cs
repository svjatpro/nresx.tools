using System;

namespace nresx.Core.Exceptions
{
    /// <summary>
    /// Base type for exceptions raised by the nresx library.
    /// Catch <see cref="NresxException"/> to handle any nresx-originated failure;
    /// catch a specific subclass to handle one kind.
    /// </summary>
    public class NresxException : Exception
    {
        public NresxException() { }
        public NresxException( string message ) : base( message ) { }
        public NresxException( string message, Exception inner ) : base( message, inner ) { }
    }
}
