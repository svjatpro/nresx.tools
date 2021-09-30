using System.Text;

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

        // todo: remove hardcode
        public static string ReplaceNewLine( this string source )
        {
            var occurs = 0;
            var sb = new StringBuilder( source.Length );
            for ( var i = 0; i < source.Length; i++ )
            {
                var ch = source[i];
                if ( ch == '\n' && ( i == 0 || source[i - 1] != '\r' ) )
                {
                    occurs++;
                    sb.Capacity += 1;
                    sb.Append( "\r\n" );
                }
                else
                {
                    sb.Append( ch );
                }
            }

            return occurs == 0 ? source : sb.ToString();
        }
    }
}
