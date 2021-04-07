using System.Collections.Generic;
using System.IO;

namespace NResx.Tools
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
    }
}
