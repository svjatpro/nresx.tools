namespace nresx.Tools.Extensions
{
    public static class StringExtensions
    {
        public static string ToFirstCapital( this string source )
        {
            if ( string.IsNullOrWhiteSpace( source ) )
                return source;

            var buff = source.ToCharArray();
            buff[0] = char.ToUpper( source[0] );

            return new string( buff );
        }
    }
}
