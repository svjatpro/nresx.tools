using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using nresx.Tools.Extensions;

namespace nresx.Tools.Formatters
{
    public enum JsonElementType
    {
        None,
        KeyValue,
        KeyObject,
        Object
    }
    public class ResourceElementJson : ResourceElement
    {
        public string Path { get; set; }
        public string KeyProperyName { get; set; }
        public string ValueProperyName { get; set; }
        public string CommentProperyName { get; set; }
        public JsonElementType ElementType { get; set; }

        
    }

    /// <summary>
    /// Json resource file formatter
    /// </summary>
    internal class FileFormatterJson : IFileFormatter
    {
        #region Private fields

        private ResourceFileOptionJson Options;

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
            el.KeyProperyName = keyToken?.name ?? KeyNames.First();
            el.Key = keyToken?.value?.ReplaceNewLine() ?? string.Empty;

            var valueToken = ValueNames
                .Select( k => new{ name = k, value = props.SingleOrDefault( p => p.Name.Trim().ToLower() == k )?.Value.Value<string>() } )
                .FirstOrDefault( k => k.value != null );
            el.ValueProperyName = valueToken?.name ?? ValueNames.First();
            el.Value = valueToken?.value?.ReplaceNewLine() ?? string.Empty;

            var commentToken = CommentNames
                .Select( k => new { name = k, value = props.SingleOrDefault( p => p.Name.Trim().ToLower() == k )?.Value.Value<string>() } )
                .FirstOrDefault( k => k.value != null );
            el.CommentProperyName = commentToken?.name ?? CommentNames.First();
            el.Comment = commentToken?.value?.ReplaceNewLine() ?? string.Empty;

            element = el;
            return !string.IsNullOrWhiteSpace( element.Key ) && 
                   ( string.IsNullOrWhiteSpace( keyToken?.name ) || 
                     string.IsNullOrWhiteSpace( Options?.KeyName ) || 
                     Options?.KeyName == el.KeyProperyName );
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

                                KeyProperyName = KeyNames.First(),
                                ValueProperyName = ValueNames.First(),
                                CommentProperyName = CommentNames.First(),

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

        public FileFormatterJson( ResourceFileOption options = null )
        {
            Options = options as ResourceFileOptionJson;
            if ( !string.IsNullOrWhiteSpace( Options?.KeyName ) )
                KeyNames = new[] { Options.KeyName }.Concat( KeyNames ).ToArray();
                //KeyNames = new[] { Options.KeyName };
            if ( !string.IsNullOrWhiteSpace( Options?.ValueName ) )
                ValueNames = new[] { Options.ValueName };
                //ValueNames = new[] { Options.ValueName }.Concat( ValueNames ).ToArray();
            if ( !string.IsNullOrWhiteSpace( Options?.CommentName ) )
                CommentNames = new[] { Options.CommentName };
                //CommentNames = new[] { Options.CommentName }.Concat( CommentNames ).ToArray();

            PropertiesMap = new HashSet<string>( KeyNames.Concat( ValueNames ).Concat( CommentNames )
                .Select( k => k.Trim().ToLower() ) );
        }

        public bool LoadResourceFile( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            if ( LoadRawElements( stream, out var raw ) )
            {
                elements = raw;
                ElementHasComment = elements?.All( el => ( (ResourceElementJson) el ).ElementType != JsonElementType.KeyValue ) ?? true;
                return true;
            }

            elements = null;
            return false;
        }
        
        public bool LoadRawElements( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            using var sr = new StreamReader( stream );
            using var reader = new JsonTextReader( sr );

            // parse json
            var result = ParseJson( reader );
            elements = result.ToList();

            return true;
        }

        public void SaveResourceFile( Stream stream, IEnumerable<ResourceElement> elements )
        {
            var root = new JObject();
            
            // add elements
            foreach ( var el in elements )
            {
                var node = new JObject();
                node.Add( "value", el.Value );
                if( !string.IsNullOrWhiteSpace( el.Comment ) )
                    node.Add( "comment", el.Comment );
                
                root.Add( el.Key, node );
            }

            using var writer = new StreamWriter( stream );
            using var jsonTextWriter = new JsonTextWriter( writer ){ Formatting = Formatting.Indented };
            root.WriteTo( jsonTextWriter );
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment { get; private set; } = true;
    }
}