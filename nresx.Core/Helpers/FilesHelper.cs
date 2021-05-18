using System;
using System.IO;
using nresx.Tools.Exceptions;
using nresx.Tools.Extensions;

namespace nresx.Tools.Helpers
{
    public class FilesHelper
    {
        public static void SearchResourceFiles( string filePattern,
            Action<FileInfo, ResourceFile> action,
            Action<FileInfo, Exception> errorHandler = null,
            bool recursive = false,
            bool createNew = false,
            bool dryRun = false,
            ResourceFormatType? type = null )
        {
            var isFileName = filePattern.IsFileName();
            var rootPath = Path.GetDirectoryName( filePattern );
            var rootDir = new DirectoryInfo( string.IsNullOrWhiteSpace( rootPath ) ? Environment.CurrentDirectory : rootPath );
            if ( !rootDir.Exists )
            {
                if ( isFileName && createNew && recursive )
                {
                    if ( !dryRun )
                    {
                        Directory.CreateDirectory( rootDir.FullName );
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException();
                }
            }

            void ProcessResourceFile( 
                string fileName, 
                Func<ResourceFile> resBuilder, 
                Action<FileInfo, ResourceFile> resAction, 
                Action<FileInfo, Exception> resErrorHandler )
            {
                var fileInfo = new FileInfo( fileName );
                try
                {
                    var resource = resBuilder();
                    action( fileInfo, resource );
                }
                catch ( Exception ex )
                {
                    if ( errorHandler != null )
                    {
                        errorHandler( fileInfo, ex );
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            if ( isFileName && !new FileInfo( filePattern ).Exists )
            {
                if ( createNew )
                {
                    if ( !ResourceFormatHelper.DetectFormatByExtension( filePattern, out var format ) && !type.HasValue )
                    {
                        throw new UnknownResourceFormatException();
                    }
                    ProcessResourceFile( filePattern, () => new ResourceFile( type ?? format ), action, errorHandler );
                }
                else
                {
                    throw new FileNotFoundException();
                }
                return;
            }

            var filesProcessed = 0;
            var mask = Path.GetFileName( filePattern );
            foreach ( var file in Directory.EnumerateFiles( rootDir.FullName, mask, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly ) )
            {
                filesProcessed++;
                ProcessResourceFile( file, () => new ResourceFile( file ), action, errorHandler );
            }

            if ( filesProcessed == 0 ) throw new FileNotFoundException();
        }

        public static void SearchFiles( string filePattern,
            Action<FileInfo> action,
            Action<FileInfo, Exception> errorHandler = null,
            bool recursive = false)
        {
            var rootPath = Path.GetDirectoryName( filePattern );
            var rootDir = new DirectoryInfo( string.IsNullOrWhiteSpace( rootPath ) ? Environment.CurrentDirectory : rootPath );
            if ( !rootDir.Exists ) throw new DirectoryNotFoundException();

            var filesProcessed = 0;
            var mask = Path.GetFileName( filePattern );
            foreach ( var file in Directory.EnumerateFiles( rootDir.FullName, mask, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly ) )
            {
                filesProcessed++;
                var fileInfo = new FileInfo( file );
                try
                {
                    action( fileInfo );
                }
                catch ( Exception ex )
                {
                    if ( errorHandler != null )
                    {
                        errorHandler( fileInfo, ex );
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if ( filesProcessed == 0 ) throw new FileNotFoundException();
        }
    }
}