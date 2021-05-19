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
            if ( path == null )
            {
                type = ResourceFormatType.NA;
                return false;
            }

            var ext = Path.GetExtension( path ).ToLower();
            if ( !TypeMap.ContainsKey( ext ) )
            {
                type = ResourceFormatType.NA;
                return false;
            }

            type = TypeMap[ext];
            return true;
        }

        public static bool DetectExtension( ResourceFormatType type, out string extension )
        {
            if ( !ExtensionsMap.ContainsKey( type ) )
            {
                extension = null;
                return false;
            }

            extension = ExtensionsMap[type];
            return true;
        }

        public static ResourceFormatType GetFormatType( string path )
        {
            var ext = Path.GetExtension( path ).ToLower();
            if ( !TypeMap.ContainsKey( ext ) )
            {
                return ResourceFormatType.NA;
            }

            var type = TypeMap[ext];
            return type;
        }
        public static string GetExtension( ResourceFormatType type )
        {
            if ( !ExtensionsMap.ContainsKey( type ) )
            {
                return null;
            }

            var extension = ExtensionsMap[type];
            return extension;
        }
    }
}
