using System.Collections.Generic;
using System.IO;

namespace nresx.Tools.Helpers
{
    public class ResourceFormatHelper
    {
        private static readonly Dictionary<string, ResourceFormatType> TypeMap =
            new Dictionary<string, ResourceFormatType>
            {
                { ".resx", ResourceFormatType.Resx },
                { ".resw", ResourceFormatType.Resw },
                { ".yml",  ResourceFormatType.Yml } ,
                { ".yaml", ResourceFormatType.Yaml }
            };

        private static readonly Dictionary<ResourceFormatType, string> ExtensionsMap =
            new Dictionary<ResourceFormatType, string>
            {
                { ResourceFormatType.Resx, ".resx" },
                { ResourceFormatType.Resw, ".resw" },
                { ResourceFormatType.Yml,  ".yml" } ,
                { ResourceFormatType.Yaml, ".yaml" }
            };

        public static bool DetectFormatByExtension( string path, out ResourceFormatType type )
        {
            var ext = Path.GetExtension( path ).ToLower();
            if ( !TypeMap.ContainsKey( ext ) )
            {
                type = ResourceFormatType.NA;
                return false;
            }

            type = TypeMap[ext];
            return true;
        }

        public static bool GetExtension( ResourceFormatType type, out string extension )
        {
            if ( !ExtensionsMap.ContainsKey( type ) )
            {
                extension = null;
                return false;
            }

            extension = ExtensionsMap[type];
            return true;
        }
    }
}
