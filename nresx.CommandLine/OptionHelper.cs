namespace NResx.Tools.CommandLine
{
    public class OptionHelper
    {
        public static bool DetectResourceFormat( string formatOption, out ResourceFormatType format )
        {
            var ext = formatOption?.StartsWith( "." ) ?? false ? formatOption : $".{formatOption}";
            var result = ResourceFormatHelper.DetectFormatByExtension( ext, out var f );
            format = f;

            return result;
        }
    }
}