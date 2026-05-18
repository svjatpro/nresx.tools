using System.Collections.Generic;
using System.IO;
using System.Text;
using nresx.Core.Extensions;

namespace nresx.Core.Formatters
{
    // CSV / TSV — translator-friendly exchange format.
    //
    // Columns: key, value, comment (in that order). First row is treated as a
    // header row when it matches the expected column names (case-insensitive);
    // otherwise the first row is treated as data.
    //
    // Quoting follows RFC 4180:
    //   - fields containing the delimiter, a double quote, CR or LF are quoted with "..."
    //   - embedded " is escaped as ""
    //   - blank lines are skipped
    //
    // CSV uses ',', TSV uses '\t'. Same parser, different delimiter.
    internal class FileFormatterCsv : IFileFormatter
    {
        private readonly char Delimiter;

        public FileFormatterCsv( char delimiter )
        {
            Delimiter = delimiter;
        }

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

            var rows = ParseRows( text );
            var result = new List<ResourceElement>();

            var firstDataRow = 0;
            if ( rows.Count > 0 && IsHeaderRow( rows[0] ) )
                firstDataRow = 1;

            for ( int i = firstDataRow; i < rows.Count; i++ )
            {
                var row = rows[i];
                if ( row.Count == 0 ) continue;

                var key = row[0];
                if ( string.IsNullOrEmpty( key ) ) continue;

                var value = row.Count > 1 ? row[1] : string.Empty;
                var comment = row.Count > 2 ? row[2] : string.Empty;

                result.Add( new ResourceElement
                {
                    Type = ResourceElementType.String,
                    Key = key,
                    Value = value.ReplaceNewLine(),
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

            writer.WriteLine( $"key{Delimiter}value{Delimiter}comment" );

            foreach ( var el in elements )
            {
                writer.WriteLine( string.Join( Delimiter.ToString(), [
                    Quote( el.Key ?? string.Empty ),
                    Quote( el.Value ?? string.Empty ),
                    Quote( el.Comment ?? string.Empty ),
                ] ) );
            }
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment => true;

        private static bool IsHeaderRow( List<string> row )
        {
            if ( row.Count < 2 ) return false;
            var k = row[0].Trim();
            var v = row[1].Trim();
            return string.Equals( k, "key", System.StringComparison.OrdinalIgnoreCase )
                && string.Equals( v, "value", System.StringComparison.OrdinalIgnoreCase );
        }

        private List<List<string>> ParseRows( string text )
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;
            bool fieldStarted = false;

            for ( int i = 0; i < text.Length; i++ )
            {
                var ch = text[i];
                if ( inQuotes )
                {
                    if ( ch == '"' )
                    {
                        if ( i + 1 < text.Length && text[i + 1] == '"' )
                        {
                            field.Append( '"' );
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append( ch );
                    }
                    continue;
                }

                if ( ch == '"' && !fieldStarted )
                {
                    inQuotes = true;
                    fieldStarted = true;
                    continue;
                }

                if ( ch == Delimiter )
                {
                    row.Add( field.ToString() );
                    field.Clear();
                    fieldStarted = false;
                    continue;
                }

                if ( ch == '\r' )
                {
                    if ( i + 1 < text.Length && text[i + 1] == '\n' ) i++;
                    FinishRow();
                    continue;
                }
                if ( ch == '\n' )
                {
                    FinishRow();
                    continue;
                }

                field.Append( ch );
                fieldStarted = true;
            }

            // tail row (no trailing newline)
            if ( field.Length > 0 || row.Count > 0 )
            {
                row.Add( field.ToString() );
                if ( !IsBlankRow( row ) ) rows.Add( row );
            }

            return rows;

            void FinishRow()
            {
                row.Add( field.ToString() );
                field.Clear();
                fieldStarted = false;
                if ( !IsBlankRow( row ) ) rows.Add( new List<string>( row ) );
                row.Clear();
            }
        }

        private static bool IsBlankRow( List<string> row )
        {
            if ( row.Count == 0 ) return true;
            foreach ( var f in row ) if ( !string.IsNullOrEmpty( f ) ) return false;
            return true;
        }

        private string Quote( string value )
        {
            if ( string.IsNullOrEmpty( value ) ) return string.Empty;
            bool needsQuoting = false;
            for ( int i = 0; i < value.Length; i++ )
            {
                var c = value[i];
                if ( c == Delimiter || c == '"' || c == '\n' || c == '\r' )
                {
                    needsQuoting = true;
                    break;
                }
            }
            if ( !needsQuoting ) return value;
            return "\"" + value.Replace( "\"", "\"\"" ) + "\"";
        }
    }
}
