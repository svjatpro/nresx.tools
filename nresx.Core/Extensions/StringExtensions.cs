using System;
using System.Collections.Generic;
using System.Linq;
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

        public static string ReplaceNewLine( this string source )
        {
            var sb = new StringBuilder( source.Length );
            var occurs = source.ForEachLine( ( start, length ) =>
            {
                if ( start > 0 )
                    sb.AppendLine();

                if ( sb.Length > start )
                    sb.Capacity += sb.Length - start;

                for ( int i = 0; i < length; i++ )
                    sb.Append( source[start + i] );
            } );

            return occurs == 0 ? source : sb.ToString();
        }
        public static string[] SplitLines( this string source )
        {
            var result = new List<string>();
            source.ForEachLine( ( start, length ) =>
            {
                result.Add( source.Substring( start, length ) );
            } );
            return result.ToArray();
        }

        public static int ForEachLine( this string source, Action<int, int> lineAction )
        {
            var endLines = new Dictionary<string, int>
            {
                {"\n", 0},
                {"\r\n", 0}
            };

            var start = 0;
            var occurs = 0;
            var lineEnds = false;
            for ( int length = 0, i = 0; i < source.Length; i++ )
            {
                var ch = source[i];
                foreach ( var pattern in endLines.OrderByDescending( e => e.Value ).Select( e => e.Key ) )
                {
                    var index = endLines[pattern];
                    if ( index < pattern.Length && pattern[index] == ch )
                    {
                        endLines[pattern]++;
                        if ( endLines[pattern] == pattern.Length )
                        {
                            length = ( i - start ) - ( pattern.Length - 1 );
                            lineEnds = true;
                            break;
                        }
                    }
                    else
                    {
                        endLines[pattern] = 0;
                    }
                }

                if ( lineEnds )
                {
                    occurs++;
                    lineAction( start, length );

                    foreach ( var pattern in endLines.Keys ) endLines[pattern] = 0;
                    lineEnds = false;
                    start = i + 1;
                    length = 0;
                }
            }

            if ( start <= source.Length )
            {
                occurs++;
                lineAction( start, source.Length - start );
            }
            
            return occurs;
        }
    }
}
