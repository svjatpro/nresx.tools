using System.Collections.Generic;
using System.IO;
using System.Text;
using nresx.Core.Extensions;

namespace nresx.Core.Formatters
{
    // Java `.properties` file (ISO-8859-1 in classic Java; UTF-8 in modern Spring/Java 9+).
    // We treat the file as UTF-8 (most modern tooling does).
    //
    // Syntax:
    //   # comment   or   ! comment
    //   key = value
    //   key : value
    //   key value      (whitespace is the separator)
    //   long.key \
    //       continues.on.next.line = value
    //   key = a\nb     (escape sequences in values)
    //
    // Spec details we intentionally simplify:
    //   - Whitespace-as-separator: supported on load, but we always emit `=` on save
    //   - Multi-line continuations via trailing `\` on lines: supported on load
    //   - Unicode `\uXXXX` escapes: supported on load + save
    internal class FileFormatterJavaProperties : IFileFormatter
    {
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
            var result = new List<ResourceElement>();
            var pendingComment = new List<string>();

            string? logical;
            while ( ( logical = ReadLogicalLine( reader ) ) != null )
            {
                var trimmed = logical.TrimStart();
                if ( trimmed.Length == 0 )
                {
                    pendingComment.Clear();
                    continue;
                }
                if ( trimmed[0] == '#' || trimmed[0] == '!' )
                {
                    pendingComment.Add( trimmed.Substring( 1 ).TrimStart() );
                    continue;
                }

                if ( !TrySplitKeyValue( logical, out var rawKey, out var rawValue ) )
                {
                    pendingComment.Clear();
                    continue;
                }

                var comment = pendingComment.Count > 0 ? string.Join( "\n", pendingComment ) : string.Empty;
                pendingComment.Clear();

                result.Add( new ResourceElement
                {
                    Type = ResourceElementType.String,
                    Key = UnescapeProperties( rawKey ),
                    Value = UnescapeProperties( rawValue ).ReplaceNewLine(),
                    Comment = comment.ReplaceNewLine(),
                } );
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

            foreach ( var el in elements )
            {
                if ( !string.IsNullOrWhiteSpace( el.Comment ) )
                {
                    foreach ( var line in el.Comment.Replace( "\r\n", "\n" ).Split( '\n' ) )
                        writer.WriteLine( $"# {line.TrimEnd()}" );
                }
                writer.WriteLine( $"{EscapeKey( el.Key ?? string.Empty )}={EscapeValue( el.Value ?? string.Empty )}" );
            }
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment => true;

        // Reads one logical line (joining lines that end with a single trailing backslash).
        private static string? ReadLogicalLine( StreamReader reader )
        {
            if ( reader.EndOfStream ) return null;

            var sb = new StringBuilder();
            while ( !reader.EndOfStream )
            {
                var line = reader.ReadLine() ?? string.Empty;
                if ( EndsWithOddBackslash( line ) )
                {
                    sb.Append( line, 0, line.Length - 1 );
                    sb.Append( '\n' );
                }
                else
                {
                    sb.Append( line );
                    return sb.ToString();
                }
            }
            return sb.ToString();
        }

        private static bool EndsWithOddBackslash( string line )
        {
            var count = 0;
            for ( int i = line.Length - 1; i >= 0 && line[i] == '\\'; i-- ) count++;
            return ( count % 2 ) == 1;
        }

        // Properties separators: `=`, `:`, or run of whitespace. Spec also allows
        // escaped separators inside the key — we honor that.
        private static bool TrySplitKeyValue( string line, out string key, out string value )
        {
            int i = 0;
            // skip leading whitespace
            while ( i < line.Length && ( line[i] == ' ' || line[i] == '\t' ) ) i++;

            var keyStart = i;
            while ( i < line.Length )
            {
                var ch = line[i];
                if ( ch == '\\' && i + 1 < line.Length ) { i += 2; continue; }
                if ( ch == '=' || ch == ':' || ch == ' ' || ch == '\t' ) break;
                i++;
            }
            key = line.Substring( keyStart, i - keyStart );

            // skip whitespace
            while ( i < line.Length && ( line[i] == ' ' || line[i] == '\t' ) ) i++;
            // skip a single separator
            if ( i < line.Length && ( line[i] == '=' || line[i] == ':' ) ) i++;
            // skip whitespace after separator
            while ( i < line.Length && ( line[i] == ' ' || line[i] == '\t' ) ) i++;

            value = i < line.Length ? line.Substring( i ) : string.Empty;
            return key.Length > 0;
        }

        private static string EscapeKey( string text )
        {
            var sb = new StringBuilder( text.Length );
            foreach ( var ch in text )
            {
                switch ( ch )
                {
                    case '\\': sb.Append( @"\\" ); break;
                    case '=':  sb.Append( @"\=" ); break;
                    case ':':  sb.Append( @"\:" ); break;
                    case ' ':  sb.Append( @"\ " ); break;
                    case '\t': sb.Append( @"\t" ); break;
                    case '\n': sb.Append( @"\n" ); break;
                    case '\r': break;
                    default:   sb.Append( ch ); break;
                }
            }
            return sb.ToString();
        }

        private static string EscapeValue( string text )
        {
            var sb = new StringBuilder( text.Length );
            foreach ( var ch in text )
            {
                switch ( ch )
                {
                    case '\\': sb.Append( @"\\" ); break;
                    case '\n': sb.Append( @"\n" ); break;
                    case '\r': break;
                    case '\t': sb.Append( @"\t" ); break;
                    default:   sb.Append( ch ); break;
                }
            }
            return sb.ToString();
        }

        private static string UnescapeProperties( string text )
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
                    case 'n':  sb.Append( '\n' ); break;
                    case 'r':  sb.Append( '\r' ); break;
                    case 't':  sb.Append( '\t' ); break;
                    case 'f':  sb.Append( '\f' ); break;
                    case '\\': sb.Append( '\\' ); break;
                    case '=':  sb.Append( '=' ); break;
                    case ':':  sb.Append( ':' ); break;
                    case ' ':  sb.Append( ' ' ); break;
                    case 'u':
                        if ( i + 4 < text.Length &&
                             int.TryParse( text.Substring( i + 1, 4 ), System.Globalization.NumberStyles.HexNumber,
                                 System.Globalization.CultureInfo.InvariantCulture, out var code ) )
                        {
                            sb.Append( (char) code );
                            i += 4;
                        }
                        else sb.Append( "\\u" );
                        break;
                    default: sb.Append( text[i] ); break;
                }
            }
            return sb.ToString();
        }
    }
}
