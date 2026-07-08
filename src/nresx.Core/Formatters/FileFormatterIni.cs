using System.Collections.Generic;
using System.IO;
using System.Text;
using nresx.Core.Extensions;

namespace nresx.Core.Formatters
{
    // INI format - old, simple, somewhat ambiguous (no formal spec).
    //
    // Supported:
    //   ; comment   or   # comment   (line-leading)
    //   [section]
    //   key=value
    //   key = value
    //
    // Section/key combine into the ResourceElement key as `section.key`.
    // Keys outside any section (the "global" section) are stored without prefix.
    // Values may be quoted with `"..."` - we strip the quotes on load and re-add
    // on save if the value contains characters that need quoting.
    internal class FileFormatterIni : IFileFormatter
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
            var section = string.Empty;

            string? line;
            while ( ( line = reader.ReadLine() ) != null )
            {
                var trimmed = line.Trim();
                if ( trimmed.Length == 0 )
                {
                    pendingComment.Clear();
                    continue;
                }
                if ( trimmed[0] == ';' || trimmed[0] == '#' )
                {
                    pendingComment.Add( trimmed.Substring( 1 ).TrimStart() );
                    continue;
                }
                if ( trimmed[0] == '[' && trimmed[trimmed.Length - 1] == ']' )
                {
                    section = trimmed.Substring( 1, trimmed.Length - 2 ).Trim();
                    pendingComment.Clear();
                    continue;
                }

                var eq = trimmed.IndexOf( '=' );
                if ( eq <= 0 )
                {
                    pendingComment.Clear();
                    continue;
                }

                var key = trimmed.Substring( 0, eq ).TrimEnd();
                var value = trimmed.Substring( eq + 1 ).TrimStart();
                value = StripSurroundingQuotes( value );

                var fullKey = string.IsNullOrEmpty( section ) ? key : $"{section}.{key}";
                var comment = pendingComment.Count > 0 ? string.Join( "\n", pendingComment ) : string.Empty;
                pendingComment.Clear();

                result.Add( new ResourceElement
                {
                    Type = ResourceElementType.String,
                    Key = fullKey,
                    Value = UnescapeIniValue( value ).ReplaceNewLine(),
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
            // We deliberately emit flat `key=value` even if keys contain dots.
            // Auto-splitting `a.b=v` into `[a]\nb=v` would lose section context
            // for later non-sectioned keys (INI sections persist until the next
            // `[..]` header). Flat output round-trips cleanly through our reader.
            using var writer = new StreamWriter( stream, new UTF8Encoding( false ), bufferSize: 1024, leaveOpen: true );

            foreach ( var el in elements )
            {
                if ( !string.IsNullOrWhiteSpace( el.Comment ) )
                {
                    foreach ( var commentLine in el.Comment.Replace( "\r\n", "\n" ).Split( '\n' ) )
                        writer.WriteLine( $"; {commentLine.TrimEnd()}" );
                }

                writer.WriteLine( $"{el.Key ?? string.Empty}={QuoteIfNeeded( EscapeIniValue( el.Value ?? string.Empty ) )}" );
            }
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment => true;

        private static string EscapeIniValue( string text )
        {
            if ( string.IsNullOrEmpty( text ) ) return text;
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

        private static string UnescapeIniValue( string text )
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
                    case 'n':  sb.Append( '\n' ); break;
                    case 'r':  sb.Append( '\r' ); break;
                    case 't':  sb.Append( '\t' ); break;
                    default:   sb.Append( '\\' ).Append( text[i] ); break;
                }
            }
            return sb.ToString();
        }

        private static string StripSurroundingQuotes( string value )
        {
            if ( value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"' )
                return value.Substring( 1, value.Length - 2 );
            return value;
        }

        private static string QuoteIfNeeded( string value )
        {
            // Quote if value starts/ends with whitespace, or contains chars that look like syntax
            if ( value.Length == 0 ) return value;
            if ( char.IsWhiteSpace( value[0] ) || char.IsWhiteSpace( value[value.Length - 1] ) ||
                 value.IndexOf( ';' ) >= 0 || value.IndexOf( '#' ) >= 0 )
            {
                return $"\"{value.Replace( "\"", "\\\"" )}\"";
            }
            return value;
        }
    }
}
