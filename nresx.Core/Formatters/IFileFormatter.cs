using System.Collections.Generic;
using System.IO;

namespace nresx.Tools.Formatters
{
    internal interface IFileFormatter
    {
        bool LoadResourceFile(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers );
        bool LoadRawElements(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers );

        void SaveResourceFile(
            Stream stream,
            IEnumerable<ResourceElement> elements,
            Dictionary<string, string>? headers,
            ResourceFileOption? options = null );

        bool ElementHasKey { get; }
        bool ElementHasComment { get; }
    }
}