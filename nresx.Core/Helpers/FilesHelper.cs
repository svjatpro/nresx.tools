using System;
using System.IO;
using System.Linq;

namespace nresx.Tools.Helpers
{
    public class FilesHelper
    {
        public static void SearchResourceFiles( 
            string filePattern, 
            Action<ResourceFile> action, 
            Action<FileInfo, Exception> errorHandler = null,
            bool recursive = false )
        {
            var rootPath = Path.GetDirectoryName( filePattern );
            var rootDir = new DirectoryInfo( string.IsNullOrWhiteSpace( rootPath ) ? Environment.CurrentDirectory : rootPath );
            if ( !rootDir.Exists )
            {
                throw new DirectoryNotFoundException(); // todo: format exception
            }
            var mask = filePattern.Split( new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries ).Last();

            foreach ( var file in Directory.EnumerateFiles( rootDir.FullName, mask, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly ) )
            {
                var fileInfo = new FileInfo( file );
                try
                {
                    var resource = new ResourceFile( fileInfo.FullName );
                    action( resource );
                }
                catch ( Exception ex )
                {
                    if ( errorHandler != null )
                        errorHandler( fileInfo, ex );
                    else
                        throw ex;
                }
            }
        }
    }
}