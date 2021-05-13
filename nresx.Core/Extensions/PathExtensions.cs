using System.IO;

namespace nresx.Tools.Extensions
{
    public static class PathExtensions
    {
        public static string GetShortPath( this string path )
        {
            if ( string.IsNullOrWhiteSpace( path ) )
                return path;

            var fileName = Path.GetFileName( path );
            var dir = new DirectoryInfo( Path.GetDirectoryName( path ) ?? string.Empty );

            if ( !dir.Exists )
                return fileName;

            return Path.Combine( dir.Name, fileName );
        }
    }
}
