using System.Collections.Generic;
using System.IO;

namespace nresx.Tools.Formatters
{
    internal interface IFileFormatter
    {
        bool LoadResourceFile( Stream stream, out IEnumerable<ResourceElement> elements );
        bool LoadElements( Stream stream, out IEnumerable<ResourceElement> elements );
        void SaveResourceFile( Stream stream, IEnumerable<ResourceElement> elements );
    }
}