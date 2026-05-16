namespace nresx.Tools.Exceptions
{
    /// <summary>
    /// Raised when a caller-supplied path and an explicit <see cref="ResourceFormatType"/>
    /// disagree (RSX-116 policy: extension + explicit format must agree when both are specified).
    /// </summary>
    public class ResourceFormatMismatchException : NresxException
    {
        public ResourceFormatMismatchException( string message ) : base( message ) { }
    }
}
