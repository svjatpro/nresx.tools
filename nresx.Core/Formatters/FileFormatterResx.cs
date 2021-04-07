using System.Collections;
using System.Collections.Generic;
using System.IO;
using NResx.Tools.Winforms;

namespace NResx.Tools.Formatters
{
    internal class FileFormatterResx : IFileFormatter
    {
        public bool LoadResourceFile( Stream stream, out List<ResourceElement> elements )
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
                    Key = item.Key.ToString(),
                    Value = nodeInfo?.ValueData ?? item.Value.ToString(),
                    Comment = nodeInfo?.Comment
                } );
            }
            elements = result;
            return true;
        }

        public void SaveResourceFile( Stream stream, List<ResourceElement> elements )
        {
            using var writer = new ResXResourceWriter( stream );
            elements.ForEach( el =>
            {
                writer.AddResource( new ResXDataNode( el.Key, el.Value ){ Comment = el.Comment } );
            } );
            writer.Generate();
            writer.Close();
        }
    }
}