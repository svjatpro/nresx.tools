using System.Collections.Generic;
using System.IO;

namespace nresx.Core.Formatters
{
    internal interface IFileFormatter
    {
        bool LoadResourceFile(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers,
            out List<Comment> comments );
        bool LoadRawElements(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers,
            out List<Comment> comments );

        void SaveResourceFile(
            Stream stream,
            IEnumerable<ResourceElement> elements,
            Dictionary<string, string> headers,
            List<Comment> comments,
            ResourceFileOption? options = null );

        bool ElementHasKey { get; }
        bool ElementHasComment { get; }
    }
}