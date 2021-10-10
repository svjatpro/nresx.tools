using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.Utilities;

namespace nresx.Tools.Formatters
{
    public class ResDoc
    {
        public List<ResItem> Items { get; set; }
    }
    public class ResItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ResConverter : IYamlTypeConverter
    {
        public IValueDeserializer ValueDeserializer { get; set; }

        public bool Accepts( Type type ) => type == typeof(ResDoc);

        public object? ReadYaml( IParser parser, Type type )
        {
            parser.Consume<MappingStart>();

            var doc = new ResDoc
            {
                Items = (List<ResItem>)ValueDeserializer.DeserializeValue( parser, typeof( List<ResItem> ), new SerializerState(), ValueDeserializer )
            };

            parser.Consume<MappingEnd>();
            return doc;
        }

        public void WriteYaml( IEmitter emitter, object? value, Type type )
        {
            throw new NotImplementedException();
        }
    }

    public class ResourceItemDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer nodeDeserializer;

        public ResourceItemDeserializer()
        {

        }

        public ResourceItemDeserializer( INodeDeserializer nodeDeserializer )
        {
            this.nodeDeserializer = nodeDeserializer;
        }

        public bool Deserialize( IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value )
        {
            if ( expectedType == typeof( List<ResItem> ) )
            {
                value = nestedObjectDeserializer( parser, expectedType );
                //value = new List<ResItem>();
                return false;
            }

            if ( expectedType != typeof( ResItem ) )
            {
                value = null;
                return false;
            }

            if ( nodeDeserializer.Deserialize( parser, expectedType, nestedObjectDeserializer, out value ) )
            {

                return true;
            }
            return false;
        }
    }

    internal class FileFormatterYaml : IFileFormatter
    {
        public bool LoadResourceFile( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            using var reader = new StreamReader( stream );
            var deserializer = new DeserializerBuilder()
                .Build();

            elements = deserializer
                .Deserialize<Dictionary<string, string>>( reader )
                .Select( el =>
                {
                    var value = el.Value;
                    if ( value.Contains( "\r\n" ) )
                        value = value.Replace( "\r\n", "\n" );
                    return new ResourceElement
                    {
                        Type = ResourceElementType.String,
                        Key = el.Key,
                        Value = value.Replace( "\n", "\r\n" )
                    };
                } )
                .ToList();

            return true;
        }

        public bool LoadRawElements( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            //var converter = new ResConverter();

            using var reader = new StreamReader( stream );
            //var deserializerBuilder = new DeserializerBuilder();
                //.WithNodeDeserializer( inner => new ResourceItemDeserializer(inner), s => s.InsteadOf<ObjectNodeDeserializer>() )
                //.WithNodeDeserializer( new ResourceItemDeserializer() ) 
                //.WithTypeConverter( converter );
            
            //converter.ValueDeserializer = deserializerBuilder.BuildValueDeserializer();
            //var deserializer = deserializerBuilder.Build();

            //var el = deserializer.Deserialize<Dictionary<string, string>>( reader );
            //var el = deserializer.Deserialize<ResDoc>( reader );


            var parser = new Parser( reader );
            var result = new List<ResourceElement>();
            Scalar key = null;
            while ( parser.MoveNext() )
            {
                if ( parser.Current is Scalar node )
                {
                    if ( node.Start.Column == 1 )
                    {
                        key = node;
                    }
                    else if ( key?.Start.Line == node.Start.Line || string.IsNullOrEmpty( key?.Value ) )
                    {
                        result.Add( new ResourceElement
                        {
                            Key = key?.Value ?? string.Empty,
                            Value = node.Value,
                            Type = ResourceElementType.String
                        } );
                        key = null;
                    }
                }
            }

            //elements = deserializer
            //    .Deserialize<Dictionary<string, string>>( reader )
            //    .Select( el =>
            //    {
            //        var value = el.Value;
            //        if ( value.Contains( "\r\n" ) )
            //            value = value.Replace( "\r\n", "\n" );
            //        return new ResourceElement
            //        {
            //            Type = ResourceElementType.String,
            //            Key = el.Key,
            //            Value = value.Replace( "\n", "\r\n" )
            //        };
            //    } )
            //    .ToList();

            elements = result;
            return true;
        }

        public void SaveResourceFile( Stream stream, IEnumerable<ResourceElement> elements )
        {
            using var writer = new StreamWriter( stream );
            var serializer = new SerializerBuilder()
                .Build();

            var body = elements.ToDictionary( el => el.Key, el => el.Value );
            serializer.Serialize( writer, body );
        }
    }
}