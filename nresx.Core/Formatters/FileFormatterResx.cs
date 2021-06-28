using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using nresx.Winforms;

namespace nresx.Tools.Formatters
{
    internal class FileFormatterResx : IFileFormatter
    {
        public bool LoadResourceFile( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            using var reader = new ResXResourceReader( stream );
            reader.UseResXDataNodes = true;
            var result = new List<ResourceElement>();
            foreach ( DictionaryEntry item in reader )
            {
                var node = item.Value as ResXDataNode;
                var nodeInfo = node?.GetDataNodeInfo();
                result.Add( new ResourceElement
                {
                    Type = ResourceElementType.String, // 
                    Key = item.Key.ToString(),
                    Value = nodeInfo?.ValueData ?? item.Value.ToString(),
                    Comment = nodeInfo?.Comment
                } );
            }
            elements = result;
            return true;
        }

        public bool LoadRawElements( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            var doc = XDocument.Load( stream );
            var entries = doc.Root?.Elements( "data" );

            elements = entries?
                .Select( e => new ResourceElement
                {
                    Key = e.Attribute( "name" )?.Value,
                    Value = e.Element( "value" )?.Value,
                    Comment = e.Element( "comment" )?.Value
                } )
                .ToList();

            return true;
        }

        public void SaveResourceFile( Stream stream, IEnumerable<ResourceElement> elements, bool validate = true )
        {
            using var writer = new ResXResourceWriter( stream );
            foreach ( var el in elements )
                writer.AddResource( new ResXDataNode( el.Key, el.Value ) { Comment = el.Comment } );
            writer.Generate();
            writer.Close();
        }
    }
}