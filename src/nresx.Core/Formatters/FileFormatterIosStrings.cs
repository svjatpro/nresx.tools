using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using nresx.Core.Extensions;

namespace nresx.Core.Formatters
{
    // iOS / macOS `.strings` format:
    //   /* Comment for next entry */
    //   "key" = "value";
    //   // also-valid line comment
    //   "key with spaces" = "Hello, %@";
    //
    // Escape sequences inside quoted strings: \" \\ \n \t \r \0 \uXXXX
    internal class FileFormatterIosStrings : IFileFormatter
    {
        // "key" = "value";   - values and keys are double-quoted; escape sequences allowed inside
        private static readonly Regex EntryRegex = new(
            @"""(?<key>(?:[^""\\]|\\.)*)""\s*=\s*""(?<value>(?:[^""\\]|\\.)*)""\s*;",
            RegexOptions.Compiled | RegexOptions.Singleline );

        public bool LoadResourceFile(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers,
            out List<Comment> comments )
        {
            return LoadRawElements( stream, out elements, out headers, out comments );
        }

        public bool LoadRawElements(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers,
            out List<Comment> comments )
        {
            headers = [];
            comments = [];

            using var reader = new StreamReader( stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true );
            var text = reader.ReadToEnd();

            var result = new List<ResourceElement>();
            var idx = 0;
            string? pendingComment = null;

            while ( idx < text.Length )
            {
                // skip whitespace
                while ( idx < text.Length && char.IsWhiteSpace( text[idx] ) ) idx++;
                if ( idx >= text.Length ) break;

                // block comment /* ... */
                if ( idx + 1 < text.Length && text[idx] == '/' && text[idx + 1] == '*' )
                {
                    var end = text.IndexOf( "*/", idx + 2, System.StringComparison.Ordinal );
                    if ( end < 0 ) break;
                    pendingComment = text.Substring( idx + 2, end - idx - 2 ).Trim();
                    idx = end + 2;
                    continue;
                }

                // line comment //
                if ( idx + 1 < text.Length && text[idx] == '/' && text[idx + 1] == '/' )
                {
                    var end = text.IndexOf( '\n', idx + 2 );
                    if ( end < 0 ) end = text.Length;
                    pendingComment = text.Substring( idx + 2, end - idx - 2 ).Trim();
                    idx = end;
                    continue;
                }

                // entry
                var match = EntryRegex.Match( text, idx );
                if ( !match.Success || match.Index != idx )
                {
                    // skip the unknown character to avoid infinite loop
                    idx++;
                    continue;
                }

                result.Add( new ResourceElement
                {
                    Type = ResourceElementType.String,
                    Key = Unescape( match.Groups["key"].Value ),
                    Value = Unescape( match.Groups["value"].Value ).ReplaceNewLine(),
                    Comment = ( pendingComment ?? string.Empty ).ReplaceNewLine(),
                } );
                pendingComment = null;
                idx = match.Index + match.Length;
            }

            elements = result;
            return true;
        }

        public void SaveResourceFile(
            Stream stream,
            IEnumerable<ResourceElement> elements,
            Dictionary<string, string> headers,
            List<Comment> comments,
            ResourceFileOption? options = null )
        {
            using var writer = new StreamWriter( stream, new UTF8Encoding( false ), bufferSize: 1024, leaveOpen: true );

            var first = true;
            foreach ( var el in elements )
            {
                if ( !first ) writer.WriteLine();
                first = false;

                if ( !string.IsNullOrWhiteSpace( el.Comment ) )
                {
                    writer.WriteLine( $"/* {el.Comment.ReplaceNewLine().Replace( "*/", "* /" )} */" );
                }

                writer.WriteLine( $"\"{Escape( el.Key ?? string.Empty )}\" = \"{Escape( el.Value ?? string.Empty )}\";" );
            }
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment => true;

        private static string Escape( string text )
        {
            if ( string.IsNullOrEmpty( text ) ) return text;
            var sb = new StringBuilder( text.Length );
            foreach ( var ch in text )
            {
                switch ( ch )
                {
                    case '\\': sb.Append( @"\\" ); break;
                    case '"':  sb.Append( "\\\"" ); break;
                    case '\n': sb.Append( @"\n" ); break;
                    case '\r': break; // strip CR; LF was already emitted via \n
                    case '\t': sb.Append( @"\t" ); break;
                    default:   sb.Append( ch ); break;
                }
            }
            return sb.ToString();
        }

        private static string Unescape( string text )
        {
            if ( string.IsNullOrEmpty( text ) ) return text;
            var sb = new StringBuilder( text.Length );
            for ( int i = 0; i < text.Length; i++ )
            {
                if ( text[i] != '\\' || i + 1 >= text.Length )
                {
                    sb.Append( text[i] );
                    continue;
                }
                i++;
                switch ( text[i] )
                {
                    case '\\': sb.Append( '\\' ); break;
                    case '"':  sb.Append( '"' ); break;
                    case '\'': sb.Append( '\'' ); break;
                    case 'n':  sb.Append( '\n' ); break;
                    case 'r':  sb.Append( '\r' ); break;
                    case 't':  sb.Append( '\t' ); break;
                    case '0':  sb.Append( '\0' ); break;
                    default:   sb.Append( '\\' ).Append( text[i] ); break;
                }
            }
            return sb.ToString();
        }
    }
}
