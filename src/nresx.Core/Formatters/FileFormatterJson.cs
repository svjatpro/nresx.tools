using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using nresx.Core.Extensions;

namespace nresx.Core.Formatters
{
    public enum JsonElementType
    {
        None,
        KeyValue,
        KeyObject,
        Object
    }
    internal class ResourceElementJson : ResourceElement
    {
        public string Path { get; set; }
        public string KeyPropertyName { get; set; }
        public string ValuePropertyName { get; set; }
        public string CommentPropertyName { get; set; }
        public JsonElementType ElementType { get; set; }
    }

    /// <summary>
    /// Json resource file formatter
    /// </summary>
    internal class FileFormatterJson : IFileFormatter
    {
        #region Private fields

        private readonly ResourceFileOptionJson? Options;

        private readonly string[] KeyNames = { "key", "id", "name" };
        private readonly string[] ValueNames = { "value", "message", "string", "text", "content", "translation" };
        private readonly string[] CommentNames = { "comment", "description", "context", "developer_comment" };
        private readonly HashSet<string> PropertiesMap;

        #endregion

        #region Private methods

        private enum NodeType
        {
            Element,
            ElementsList,
            Object,
            Value
        }

        private List<ResourceElementJson> ParseJson( JsonTextReader reader )
        {
            var elements = new List<ResourceElementJson>();
            ParseJson( reader, elements, out var type );
            QualifyKeysByPath( elements );

            return elements;
        }

        // Elements from different subtrees can share a leaf name ("a.title" vs "b.title"),
        // which used to surface as false Duplicate findings and strict-load failures on
        // ordinary nested files (RSX-251). Qualify keys with the path RELATIVE to the
        // common prefix of all elements, dot-joined (the i18next convention): files whose
        // elements live under one subtree keep their plain leaf keys, mixed files get
        // unique "section.leaf" keys.
        private static void QualifyKeysByPath( List<ResourceElementJson> elements )
        {
            if ( elements.Count == 0 )
                return;

            var paths = elements
                .Select( el => ( el.Path ?? string.Empty ).Split( new[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries ) )
                .ToList();

            var common = paths[0].Length;
            foreach ( var p in paths.Skip( 1 ) )
            {
                var i = 0;
                while ( i < common && i < p.Length && paths[0][i] == p[i] ) i++;
                common = i;
                if ( common == 0 ) break;
            }

            for ( var e = 0; e < elements.Count; e++ )
            {
                var relative = paths[e].Skip( common ).ToArray();
                if ( relative.Length > 0 )
                    elements[e].Key = string.Join( ".", relative.Append( elements[e].Key ) );
            }
        }

        private bool ParseElementNode( JObject node, string path, JsonElementType elType, out ResourceElementJson element )
        {
            var el = new ResourceElementJson();

            el.Type = ResourceElementType.String;
            el.Path = path;
            el.ElementType = elType;

            var props = node?.Children<JProperty>() ?? new JEnumerable<JProperty>();
            if ( !props.Any() )
                props = node?.Children<JObject>().FirstOrDefault()?.Children<JProperty>() ?? new JEnumerable<JProperty>();

            var keyToken = KeyNames
                .Select( k => new{ name = k, value = props.SingleOrDefault( p => p.Name.Trim().ToLower() == k )?.Value.Value<string>() } )
                .FirstOrDefault( k => k.value != null );
            el.KeyPropertyName = keyToken?.name ?? KeyNames.First();
            el.Key = keyToken?.value?.ReplaceNewLine() ?? string.Empty;

            var valueToken = ValueNames
                .Select( k => new{ name = k, value = props.SingleOrDefault( p => p.Name.Trim().ToLower() == k )?.Value.Value<string>() } )
                .FirstOrDefault( k => k.value != null );
            el.ValuePropertyName = valueToken?.name ?? ValueNames.First();
            el.Value = valueToken?.value?.ReplaceNewLine() ?? string.Empty;

            var commentToken = CommentNames
                .Select( k => new { name = k, value = props.SingleOrDefault( p => p.Name.Trim().ToLower() == k )?.Value.Value<string>() } )
                .FirstOrDefault( k => k.value != null );
            el.CommentPropertyName = commentToken?.name ?? CommentNames.First();
            el.Comment = commentToken?.value?.ReplaceNewLine() ?? string.Empty;

            element = el;
            return !string.IsNullOrWhiteSpace( element.Key ) && 
                   ( string.IsNullOrWhiteSpace( keyToken?.name ) || 
                     string.IsNullOrWhiteSpace( Options?.KeyName ) || 
                     Options?.KeyName == el.KeyPropertyName );
        }

        private JToken ParseJson( JsonTextReader reader, List<ResourceElementJson> elements, out NodeType type )
        {
            if ( reader.TokenType == JsonToken.None ) reader.Read();
            var path = reader.Path;

            // parse object
            if ( reader.TokenType == JsonToken.StartObject )
            {
                reader.Read();
                var children = new List<JToken>();
                var plainProps = new List<( string name, JToken value )>();
                var hasElements = false;

                while ( reader.TokenType != JsonToken.EndObject )
                {
                    var propName = (string) reader.Value;
                    reader.Read();

                    var item = ParseJson( reader, elements, out var childType );

                    // leaf property - element-vs-container decision is made AFTER the
                    // object completes; deciding per-property used to silently drop every
                    // plain key that followed a metadata-named one ("comment" etc.) in a
                    // mixed container (RSX-251)
                    if ( childType == NodeType.Value )
                    {
                        plainProps.Add( ( propName, item ) );
                    }
                    // object or "key : object"
                    else if ( childType == NodeType.Element && item.Type == JTokenType.Object )
                    {
                        hasElements = true;
                        ( (JObject) item ).Add( KeyNames.First(), propName ); // todo: figure out possible conflict with existing key property
                        children.Add( item );
                    }

                    reader.Read();
                }

                // an ELEMENT node holds ONLY metadata-named leaves and at least one value
                // property; anything else is a key:value container - a container may
                // legitimately contain keys named "comment", "text", ... (RSX-251)
                var isElementNode =
                    plainProps.Count > 0 &&
                    plainProps.All( p => PropertiesMap.Contains( p.name?.Trim().ToLower() ) ) &&
                    plainProps.Any( p => ValueNames.Contains( p.name?.Trim().ToLower() ) );

                if ( isElementNode )
                {
                    foreach ( var ( name, value ) in plainProps )
                        children.Add( new JProperty( name, value ) );
                }
                else
                {
                    foreach ( var ( name, value ) in plainProps )
                    {
                        if ( Options?.Path == null || Options?.Path == path )
                        {
                            hasElements = true;
                            elements.Add( new ResourceElementJson
                            {
                                Key = name,
                                Value = value.Value<string>()?.ReplaceNewLine(),

                                KeyPropertyName = KeyNames.First(),
                                ValuePropertyName = ValueNames.First(),
                                CommentPropertyName = CommentNames.First(),

                                Path = path,
                                Type = ResourceElementType.String,
                                ElementType = JsonElementType.KeyValue
                            } );
                        }
                    }
                }

                JToken obj;
                if ( !isElementNode && hasElements )
                {
                    obj = new JArray();
                    foreach ( var child in children )
                    {
                        if ( child is not JObject jObj ) continue;
                        if ( ParseElementNode( jObj, path, JsonElementType.KeyObject, out var el ) &&
                             ( Options?.Path == null || Options?.Path == path ) )
                        {
                            elements.Add( el );
                        }
                        ( (JArray)obj ).Add( child );
                    }
                    type = NodeType.ElementsList;
                }
                else
                {
                    obj = new JObject();
                    foreach ( var child in children )
                        ( (JObject) obj ).Add( child );

                    type = isElementNode ? NodeType.Element : NodeType.Object;
                }

                return obj;
            }

            // parse array
            if ( reader.TokenType == JsonToken.StartArray )
            {
                reader.Read();
                var array = new JArray();
                var hasElements = false;
                while ( reader.TokenType != JsonToken.EndArray )
                {
                    array.Add( ParseJson( reader, elements, out var childType ) );
                    if ( childType == NodeType.Element ) 
                        hasElements = true;
                    reader.Read();
                }

                if ( hasElements )
                {
                    foreach ( var token in array )
                    {
                        if ( token is not JObject jToken ) continue;
                        if ( ParseElementNode( jToken, path, JsonElementType.Object, out var el ) &&
                             ( Options?.Path == null || Options?.Path == path ) )
                            elements.Add( el );
                    }
                    type = NodeType.ElementsList;
                }
                else
                {
                    type = NodeType.Object;
                }

                return array;
            }

            // parse as plain value (considered as a leaf)
            type = NodeType.Value;
            return new JValue( reader.Value );
        }

        #endregion

        public FileFormatterJson( ResourceFileOption? options = null )
        {
            Options = options as ResourceFileOptionJson;
            if ( !string.IsNullOrWhiteSpace( Options?.KeyName ) )
                KeyNames = new[] { Options!.KeyName }.Concat( KeyNames ).ToArray();
                //KeyNames = new[] { Options.KeyName };
            if ( !string.IsNullOrWhiteSpace( Options?.ValueName ) )
                ValueNames = [Options!.ValueName];
                //ValueNames = new[] { Options.ValueName }.Concat( ValueNames ).ToArray();
            if ( !string.IsNullOrWhiteSpace( Options?.CommentName ) )
                CommentNames = [Options!.CommentName];
                //CommentNames = new[] { Options.CommentName }.Concat( CommentNames ).ToArray();

            PropertiesMap = [..KeyNames
                .Concat( ValueNames )
                .Concat( CommentNames )
                .Select( k => k.Trim().ToLower() )];
        }

        public bool LoadResourceFile(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers,
            out List<Comment> comments )
        {
            if ( LoadRawElements( stream, out var raw, out headers, out comments ) )
            {
                elements = raw;
                ElementHasComment = elements?.All( el => ( (ResourceElementJson) el ).ElementType != JsonElementType.KeyValue ) ?? true;
                return true;
            }

            elements = [];
            return false;
        }
        
        public bool LoadRawElements(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers,
            out List<Comment> comments )
        {
            // leaveOpen: true - caller owns the stream
            using var sr = new StreamReader( stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true );
            using var reader = new JsonTextReader( sr );

            // parse json
            var result = ParseJson( reader );
            elements = result.ToList();

            // 
            headers = [];
            comments = [];

            return true;
        }

        // Chooses the output shape when the caller did not force one. On a json->json roundtrip
        // the loader recorded each element's shape (ElementType), so a uniform source shape is
        // preserved. Elements arriving from another format (resx, po, ...) carry no recorded shape:
        // default to flat key:value - the common i18next shape - and upgrade to key:object only when
        // a comment needs somewhere to live, so it is not silently dropped (RSX-253). This also
        // replaces the old unconditional KeyObject default that turned every flat file into
        // "key": { "value": ... } on the way back out.
        private static JsonElementType DetermineElementType( List<ResourceElement> elements, ResourceFileOptionJson? jsonOption )
        {
            if ( jsonOption != null && jsonOption.ElementType != JsonElementType.None )
                return jsonOption.ElementType;

            // Preserve a uniform recorded shape for the two flat-ish layouts (key:value, key:object).
            // The array-of-objects Object layout wraps elements in a "strings" array and always
            // requires an explicit ElementType, so it is not inferred implicitly.
            var recorded = elements
                .Select( el => ( el as ResourceElementJson )?.ElementType ?? JsonElementType.None )
                .Where( t => t != JsonElementType.None )
                .Distinct()
                .ToList();
            if ( recorded.Count == 1 && recorded[0] != JsonElementType.Object && elements.All( el => el is ResourceElementJson ) )
                return recorded[0];

            if ( elements.Any( el => !string.IsNullOrWhiteSpace( el.Comment ) ) )
                return JsonElementType.KeyObject;

            return JsonElementType.KeyValue;
        }

        public void SaveResourceFile(
            Stream stream,
            IEnumerable<ResourceElement> elements,
            Dictionary<string, string> headers,
            List<Comment> comments,
            ResourceFileOption? options = null )
        {
            var root = new JObject();
            var elementsRoot = root;
            var jsonOption = options as ResourceFileOptionJson;
            if ( !string.IsNullOrWhiteSpace( jsonOption?.Path ) )
            {
                var pathParts = jsonOption.Path.Split( '.' );
                foreach ( var part in pathParts )
                {
                    var pathNode = new JObject();
                    elementsRoot.Add( part, pathNode );
                    elementsRoot = pathNode;
                }
            }

            var elementList = elements.ToList();
            var shape = DetermineElementType( elementList, jsonOption );

            JArray arrayRoot = null;
            if ( shape == JsonElementType.Object )
            {
                arrayRoot = new JArray();
                elementsRoot.Add( "strings", arrayRoot );
            }

            // add elements
            foreach ( var el in elementList )
            {
                var elJson = el as ResourceElementJson;
                var key = jsonOption?.KeyName ?? elJson?.KeyPropertyName ?? KeyNames.First();
                var value = jsonOption?.ValueName ?? elJson?.ValuePropertyName ?? ValueNames.First();
                var comment = jsonOption?.CommentName ?? elJson?.CommentPropertyName ?? CommentNames.First();
                JObject node;

                switch ( shape )
                {
                    case JsonElementType.KeyObject:
                        node = new JObject { { value, el.Value } };
                        if ( !string.IsNullOrWhiteSpace( el.Comment ) )
                            node.Add( comment, el.Comment );
                        elementsRoot.Add( el.Key, node );
                        break;
                    case JsonElementType.KeyValue:
                        elementsRoot.Add( el.Key, el.Value );
                        break;
                    case JsonElementType.Object:
                        node = new JObject
                        {
                            { key, el.Key },
                            { value, el.Value }
                        };
                        if ( !string.IsNullOrWhiteSpace( el.Comment ) )
                            node.Add( comment, el.Comment );
                        arrayRoot?.Add( node );
                        break;
                }
            }

            // leaveOpen: true - caller owns the stream
            using var writer = new StreamWriter( stream, new UTF8Encoding( false ), bufferSize: 1024, leaveOpen: true );
            using var jsonTextWriter = new JsonTextWriter( writer ){ Formatting = Formatting.Indented };
            root.WriteTo( jsonTextWriter );
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment { get; private set; } = true;
    }
}