using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace nresx.Tools.Formatters
{
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