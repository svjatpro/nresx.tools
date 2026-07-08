using nresx.Core;
using nresx.Core.Extensions;
using nresx.Core.Helpers;

namespace nresx.CommandLine
{
    public class OptionHelper
    {
        public static bool DetectResourceFormat( string formatOption, out ResourceFormatType format )
        {
            var ext = formatOption.ToExtension();
            var result = ResourceFormatHelper.DetectFormatByExtension( ext, out var f );
            format = f;

            return result;
        }

        public static string GetFormatOption( ResourceFormatType type )
        {
            if( ResourceFormatHelper.DetectExtension( type, out var f ) )
                return f.StartsWith( "." ) ? f.Substring( 1 ) : f;
            return null;
        }
    }
}