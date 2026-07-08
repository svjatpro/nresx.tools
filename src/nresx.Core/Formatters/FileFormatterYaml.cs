using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using nresx.Core.Extensions;

namespace nresx.Core.Formatters;

// YAML formatter - supports `key: value` and block-scalar values (`|`, `>`).
// Comments: `# ...` lines immediately preceding an entry (blank-line-bounded)
// attach as that entry's comment. Inline (`key: value # foo`) and trailing
// comments are not preserved.
internal class FileFormatterYaml : IFileFormatter
{
    public bool LoadResourceFile(
        Stream stream,
        out IEnumerable<ResourceElement> elements,
        out Dictionary<string, string> headers,
        out List<Comment> comments )
    {
        headers = [];
        comments = [];
        elements = ParseYaml( stream, unique: true );
        return true;
    }

    public bool LoadRawElements(
        Stream stream,
        out IEnumerable<ResourceElement> elements,
        out Dictionary<string, string> headers,
        out List<Comment> comments )
    {
        headers = [];
        comments = [];
        elements = ParseYaml( stream, unique: false );
        return true;
    }

    public void SaveResourceFile(
        Stream stream,
        IEnumerable<ResourceElement> elements,
        Dictionary<string, string> headers,
        List<Comment> comments,
        ResourceFileOption? options = null )
    {
        // leaveOpen: true - caller owns the stream
        using var writer = new StreamWriter( stream, new UTF8Encoding( false ), bufferSize: 1024, leaveOpen: true );
        var serializer = new SerializerBuilder().Build();

        foreach ( var el in elements )
        {
            if ( !string.IsNullOrWhiteSpace( el.Comment ) )
            {
                foreach ( var line in el.Comment.Replace( "\r\n", "\n" ).Split( '\n' ) )
                    writer.WriteLine( $"# {line.TrimEnd()}" );
            }

            // serialize one key:value at a time - Serializer handles quoting,
            // escaping, block-scalar selection for multi-line values
            var single = new Dictionary<string, string> { { el.Key ?? string.Empty, el.Value ?? string.Empty } };
            var serialized = serializer.Serialize( single ).TrimEnd();
            writer.WriteLine( serialized );
        }
    }

    public bool ElementHasKey => true;
    public bool ElementHasComment => true;

    // Parses the YAML stream into ResourceElements with comment attachment.
    // `unique` = drop duplicate keys (LoadResourceFile semantics); false = preserve all (LoadRawElements semantics).
    private static List<ResourceElement> ParseYaml( Stream stream, bool unique )
    {
        using var reader = new StreamReader( stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true );
        var text = reader.ReadToEnd();

        var pairs = ExtractTopLevelPairs( text );
        var commentsByLine = ScanComments( text );

        var result = new List<ResourceElement>( pairs.Count );
        var seen = new HashSet<string>();

        foreach ( var (key, value, keyLine) in pairs )
        {
            if ( unique && !seen.Add( key ) ) continue;

            commentsByLine.TryGetValue( keyLine, out var comment );
            result.Add( new ResourceElement
            {
                Type = ResourceElementType.String,
                Key = key,
                Value = ( value ?? string.Empty ).ReplaceNewLine(),
                Comment = ( comment ?? string.Empty ).ReplaceNewLine(),
            } );
        }

        return result;
    }

    // Walks YAML events and extracts top-level (key, value, keyLineNumber) tuples.
    // Skips nested mappings/sequences - they'd be a deeper structural change.
    private static List<(string key, string value, int keyLine)> ExtractTopLevelPairs( string text )
    {
        var result = new List<(string, string, int)>();
        var parser = new Parser( new StringReader( text ) );

        int depth = 0;
        string? currentKey = null;
        int currentKeyLine = 0;

        while ( parser.MoveNext() )
        {
            switch ( parser.Current )
            {
                case MappingStart:
                case SequenceStart:
                    depth++;
                    break;
                case MappingEnd:
                case SequenceEnd:
                    depth--;
                    break;
                case Scalar scalar when depth == 1:
                    if ( currentKey == null )
                    {
                        currentKey = scalar.Value;
                        currentKeyLine = (int) scalar.Start.Line;
                    }
                    else
                    {
                        result.Add( (currentKey, scalar.Value, currentKeyLine) );
                        currentKey = null;
                    }
                    break;
            }
        }

        return result;
    }

    // Returns: keyLineNumber → multi-line comment text (lines joined with \n).
    // A run of `#` comment lines is associated with the first non-blank,
    // non-comment line that follows them. A blank line resets the buffer.
    private static Dictionary<int, string> ScanComments( string text )
    {
        var result = new Dictionary<int, string>();
        var pending = new List<string>();
        var lines = text.Replace( "\r\n", "\n" ).Split( '\n' );

        for ( int i = 0; i < lines.Length; i++ )
        {
            var trimmed = lines[i].TrimStart();
            var lineNumber = i + 1;

            if ( trimmed.StartsWith( "#" ) )
            {
                pending.Add( trimmed.Substring( 1 ).TrimStart() );
            }
            else if ( string.IsNullOrWhiteSpace( trimmed ) )
            {
                pending.Clear();
            }
            else
            {
                if ( pending.Count > 0 )
                {
                    result[lineNumber] = string.Join( "\n", pending );
                    pending.Clear();
                }
            }
        }

        return result;
    }
}
