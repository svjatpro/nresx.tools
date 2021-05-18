using System;
using System.Globalization;
using System.IO;
using System.Linq;

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

            //if ( !dir.Exists ) // can break dry run usage
            //    return fileName;

            return Path.Combine( dir.Name, fileName );
        }

        public static string GetDirectoryName( this string path )
        {
            var dir = Path.GetDirectoryName( path );
            return dir != null ? new DirectoryInfo( dir ).Name : null;
        }

        public static bool TryToExtractCultureFromPath( this string path, out CultureInfo culture )
        {
            CultureInfo TryGetCulture( Func<bool> validator, Func<string> getCode )
            {
                if ( !validator() )
                    return null;
                try
                {
                    var c = CultureInfo.GetCultureInfo( getCode() );
                    return ( c.CultureTypes & CultureTypes.UserCustomCulture ) != CultureTypes.UserCustomCulture ? c : null;
                }
                catch
                {
                    return null;
                }
            }
            var fileName = Path.GetFileNameWithoutExtension( path );
            var fileParts = fileName.Split( new[] {'.', '_', ' '}, StringSplitOptions.RemoveEmptyEntries );
            var dir = path.GetDirectoryName();

            culture = 
                TryGetCulture( () => fileParts.Length > 0, () => fileParts.First() ) ??
                TryGetCulture( () => fileParts.Length > 1, () => fileParts.Last() ) ??
                TryGetCulture( () => dir != null && ( dir.Length == 5 || dir.Length == 2 ), () => dir );

            return culture != null;
        }
        
        public static bool IsFileName( this string path )
        {
            return
                !string.IsNullOrWhiteSpace( path ) &&
                !path.Contains( '*' ) &&
                !path.Contains( '?' );
        }

        public static bool ContainsDir( this string path, params string[] containsDir )
        {
            return Path.GetDirectoryName( path )?
                .Split( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar )
                .Any( containsDir.Contains ) ?? false;
        }
    }
}
