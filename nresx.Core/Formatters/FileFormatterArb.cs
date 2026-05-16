#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using nresx.Tools.Extensions;

namespace nresx.Tools.Formatters
{
    // Flutter ARB (Application Resource Bundle) — JSON-based localization format.
    //
    // Shape:
    //   {
    //     "@@locale": "en",                      // global metadata (ignored on load)
    //     "greeting": "Hello",                   // string entry
    //     "@greeting": { "description": "..." }, // optional per-key metadata block
    //     ...
    //   }
    //
    // We round-trip strings only. The `@key` metadata description (if present)
    // is used as the element's Comment; other metadata fields
    // (placeholders, type, context) are not represented and are dropped on Save
    // — same lossy roundtrip pattern as XLIFF. Documented in the epic file.
    internal class FileFormatterArb : IFileFormatter
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
            using var jr = new JsonTextReader( reader );

            var result = new List<ResourceElement>();
            var descriptions = new Dictionary<string, string>();
            var rawEntries = new List<(string Key, string Value)>();

            try
            {
                // Token-level scan preserves duplicate keys (which JObject.Parse would collapse).
                // We only read top-level properties of the root object.
                if ( !jr.Read() || jr.TokenType != JsonToken.StartObject )
                {
                    elements = result;
                    return false;
                }

                while ( jr.Read() && jr.TokenType != JsonToken.EndObject )
                {
                    if ( jr.TokenType != JsonToken.PropertyName ) continue;
                    var name = (string)jr.Value!;

                    if ( !jr.Read() ) break;

                    if ( name.StartsWith( "@@" ) )
                    {
                        jr.Skip();
                        continue;
                    }
                    if ( name.StartsWith( "@" ) )
                    {
                        if ( jr.TokenType == JsonToken.StartObject )
                        {
                            var meta = JObject.Load( jr );
                            if ( meta["description"] is JValue desc && desc.Value != null )
                            {
                                descriptions[name.Substring( 1 )] = desc.Value.ToString() ?? string.Empty;
                            }
                        }
                        else
                        {
                            jr.Skip();
                        }
                        continue;
                    }

                    if ( jr.TokenType == JsonToken.String )
                    {
                        rawEntries.Add( (name, jr.Value?.ToString() ?? string.Empty) );
                    }
                    else
                    {
                        jr.Skip();
                    }
                }
            }
            catch ( JsonException )
            {
                elements = result;
                return false;
            }

            foreach ( var (key, value) in rawEntries )
            {
                descriptions.TryGetValue( key, out var comment );
                result.Add( new ResourceElement
                {
                    Type = ResourceElementType.String,
                    Key = key,
                    Value = value.ReplaceNewLine(),
                    Comment = ( comment ?? string.Empty ).ReplaceNewLine(),
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
            var root = new JObject();
            foreach ( var el in elements )
            {
                if ( string.IsNullOrEmpty( el.Key ) ) continue;
                root[el.Key!] = el.Value ?? string.Empty;
                if ( !string.IsNullOrWhiteSpace( el.Comment ) )
                {
                    root["@" + el.Key] = new JObject { ["description"] = el.Comment };
                }
            }

            using var writer = new StreamWriter( stream, new UTF8Encoding( false ), bufferSize: 1024, leaveOpen: true );
            using var jw = new JsonTextWriter( writer )
            {
                Formatting = Formatting.Indented,
                CloseOutput = false,
            };
            root.WriteTo( jw );
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment => true;
    }
}
