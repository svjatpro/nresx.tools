using nresx.Core.Formatters;

namespace nresx.Core.Helpers
{
    public class ResourceFormatHelper
    {
        public static bool DetectFormatByExtension( string path, out ResourceFormatType type )
        {
            if ( !FormatRegistry.TryGetByExtension( path, out var descriptor ) )
            {
                type = ResourceFormatType.NA;
                return false;
            }

            type = descriptor!.Type;
            return true;
        }

        public static bool DetectExtension( ResourceFormatType type, out string extension )
        {
            if ( !FormatRegistry.TryGetByType( type, out var descriptor ) )
            {
                extension = null!;
                return false;
            }

            extension = descriptor!.Extension;
            return true;
        }

        public static ResourceFormatType GetFormatType( string path )
        {
            return FormatRegistry.TryGetByExtension( path, out var descriptor )
                ? descriptor!.Type
                : ResourceFormatType.NA;
        }

        public static string GetExtension( ResourceFormatType type )
        {
            return FormatRegistry.TryGetByType( type, out var descriptor )
                ? descriptor!.Extension
                : null!;
        }
    }
}
