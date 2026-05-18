namespace nresx.Core.Exceptions
{
    /// <summary>
    /// Raised when nresx is asked to operate on a format it doesn't recognize —
    /// either by file extension or by an explicit <see cref="ResourceFormatType"/>.
    /// </summary>
    public class UnknownResourceFormatException : NresxException
    {
        public UnknownResourceFormatException() { }

        public UnknownResourceFormatException( string message ) : base( message ) { }
    }
}
