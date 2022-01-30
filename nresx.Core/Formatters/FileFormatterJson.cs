using System;
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
        KeyValue,
        KeyObject,
        Object
    }
    internal class ResourceElementJson : ResourceElement
    {
        public string Path { get; set; }
        public string KeyProperyName { get; set; }
        public string NameProperyName { get; set; }
        public string CommentProperyName { get; set; }
        public JsonElementType ElementType { get; set; }

    }

    /// <summary>
    /// Json resource file formatter
    /// </summary>
    internal class FileFormatterJson : IFileFormatter
    {
        #region Private fields

        private readonly string[] KeyNames = { "key", "id", "name" };
        private readonly string[] ValueNames = { "message", "string", "text", "value", "content", "translation" };
        private readonly string[] CommentNames = { "description", "context", "comment", "developer_comment" };
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

        private bool ParseElementNode( JObject node, out ResourceElementJson element )
        {
            var el = new ResourceElementJson();

            var props = node?.Children<JProperty>() ?? new JEnumerable<JProperty>();
            if ( !props.Any() )
                props = node?.Children<JObject>().FirstOrDefault()?.Children<JProperty>() ?? new JEnumerable<JProperty>();
            el.Key = KeyNames
                .Select( k => props.SingleOrDefault( p => p.Name.Trim().ToLower() == k )?.Value.Value<string>() )
                .FirstOrDefault( k => k != null )?.ReplaceNewLine() ?? string.Empty;

            el.Value = ValueNames
                .Select( k => props.SingleOrDefault( p => p.Name.Trim().ToLower() == k )?.Value.Value<string>() )
                .FirstOrDefault( k => k != null )?.ReplaceNewLine() ?? string.Empty;

            el.Comment = CommentNames
                .Select( k => props.SingleOrDefault( p => p.Name.Trim().ToLower() == k )?.Value.Value<string>() )
                .FirstOrDefault( k => k != null )?.ReplaceNewLine() ?? string.Empty;

            element = el;
            return !string.IsNullOrWhiteSpace( element.Key );
        }

        private JToken ParseJson( JsonTextReader reader, List<ResourceElementJson> elements, out NodeType type )
        {
            if ( reader.TokenType == JsonToken.None ) reader.Read();

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

                    if ( childType == NodeType.Value && PropertiesMap.Contains( propName.Trim().ToLower() ) )
                    {
                        hasElProperties = true;
                        children.Add( new JProperty( propName, item ) );
                    }
                    // plain structure "key: value"
                    else if ( childType == NodeType.Value && !PropertiesMap.Contains( propName.Trim().ToLower() ) )
                    {
                        hasElements = true;
                        children.Add( new JObject( 
                            new JProperty( KeyNames.First(), propName ),
                            new JProperty( ValueNames.First(), item ) ) );
                    }
                    else
                    {
                        children.Add( item );
                    }

                    if ( childType == NodeType.Element && item.Type == JTokenType.Object )
                    {
                        hasElements = true;
                        ( (JObject) item ).Add( KeyNames.First(), propName );
                    }

                    reader.Read();
                }
                
                JToken obj;
                if ( hasElements )
                {
                    obj = new JArray();
                    foreach ( var child in children )
                    {
                        if ( ParseElementNode( (JObject) child, out var el ) )
                            elements.Add( el );
                        ( (JArray) obj ).Add( child );
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
                    if ( childType == NodeType.Element ) hasElements = true;
                    reader.Read();
                }

                type = hasElements ? NodeType.ElementsList : NodeType.Object;
                return array;
            }

            // parse as plain value (considered as a leaf)
            type = NodeType.Value;
            return new JValue( reader.Value );
        }

        #endregion

        public FileFormatterJson()
        {
            PropertiesMap = new HashSet<string>( KeyNames.Concat( ValueNames ).Concat( CommentNames )
                .Select( k => k.Trim().ToLower() ) );
        }

        public bool LoadResourceFile( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            if ( LoadRawElements( stream, out var raw ) )
            {
                elements = raw;
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
        public bool ElementHasComment => true; //
    }
}