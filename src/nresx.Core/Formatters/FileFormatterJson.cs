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

            return elements;
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
                var hasElements = false;
                var hasElProperties = false;

                while ( reader.TokenType != JsonToken.EndObject )
                {
                    var propName = (string) reader.Value;
                    reader.Read();

                    var item = ParseJson( reader, elements, out var childType );

                    // element property
                    if ( childType == NodeType.Value && PropertiesMap.Contains( propName?.Trim().ToLower() ) )
                    {
                        hasElProperties = true;
                        children.Add( new JProperty( propName, item ) );
                    }
                    // plain structure "key: value"
                    else if ( !hasElProperties && childType == NodeType.Value && !PropertiesMap.Contains( propName?.Trim().ToLower() ) )
                    {
                        if( ( Options?.Path == null || Options?.Path == path ) )
                        {
                            hasElements = true;
                            elements.Add( new ResourceElementJson
                            {
                                Key = propName,
                                Value = item.Value<string>()?.ReplaceNewLine(),

                                KeyPropertyName = KeyNames.First(),
                                ValuePropertyName = ValueNames.First(),
                                CommentPropertyName = CommentNames.First(),

                                Path = path,
                                Type = ResourceElementType.String,
                                ElementType = JsonElementType.KeyValue
                            } );
                        }
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

                JToken obj;
                if ( hasElements )
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

                    type = hasElProperties ? NodeType.Element : NodeType.Object;
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

            JArray arrayRoot = null;
            if ( jsonOption?.ElementType == JsonElementType.Object )
            {
                arrayRoot = new JArray();
                elementsRoot.Add( "strings", arrayRoot );
            }
            
            // add elements
            foreach ( var el in elements )
            {
                var elJson = el as ResourceElementJson;
                var key = jsonOption?.KeyName ?? elJson?.KeyPropertyName ?? KeyNames.First();
                var value = jsonOption?.ValueName ?? elJson?.ValuePropertyName ?? ValueNames.First();
                var comment = jsonOption?.CommentName ?? elJson?.CommentPropertyName ?? CommentNames.First();
                JObject node;

                switch ( jsonOption?.ElementType ?? JsonElementType.KeyObject )
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