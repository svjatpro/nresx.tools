using System;
using System.IO;
using nresx.Tools.Exceptions;
using nresx.Tools.Extensions;

namespace nresx.Tools.Helpers
{
    public class FilesSearchContext
    {
        public readonly string PathSpec;
        public readonly FileInfo CurrentFile;
        public readonly int FilesProcessed;
        public readonly int FilesFaled;

        public FilesSearchContext( string pathSpec, int filesProcessed = 0, int filesFaled = 0 )
        {
            PathSpec = pathSpec;
            if ( PathSpec.IsFileName() ) CurrentFile = new FileInfo( pathSpec );
            FilesProcessed = filesProcessed;
            FilesFaled = filesFaled;
        }

        public string FileName => CurrentFile?.Name ?? Path.GetFileName( PathSpec);
        public string FullName => CurrentFile?.FullName ?? PathSpec;
    }
    public class FilesHelper
    {
        public static void SearchResourceFiles( string filePattern,
            Action<FilesSearchContext, ResourceFile> action,
            Action<FilesSearchContext, Exception> errorHandler = null,
            bool recursive = false,
            bool createNew = false,
            bool dryRun = false,
            string formatOption = null )
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
                    var exc = new DirectoryNotFoundException();
                    if ( errorHandler != null )
                    {
                        errorHandler( new FilesSearchContext( rootDir.FullName ), exc );
                        return;
                    }
                    throw exc;
                }
            }

            bool ProcessResourceFile( 
                FilesSearchContext context, 
                Func<ResourceFile> resBuilder, 
                Action<FilesSearchContext, ResourceFile> resAction, 
                Action<FilesSearchContext, Exception> resErrorHandler )
            {
                try
                {
                    var resource = resBuilder();
                    action( context, resource );
                    return true;
                }
                catch ( Exception ex )
                {
                    if ( errorHandler != null )
                    {
                        errorHandler( context, ex );
                        return false;
                    }
                    throw;
                }
            }
            
            if ( isFileName && !new FileInfo( filePattern ).Exists )
            {
                if ( createNew )
                {
                    ResourceFormatType format;
                    if ( ResourceFormatHelper.DetectFormatByExtension( formatOption, out var formatByOption ) )
                    {
                        format = formatByOption;
                    }
                    else if ( ResourceFormatHelper.DetectFormatByExtension( filePattern, out var formatByExtension ) )
                    {
                        format = formatByExtension;
                    }
                    else
                    {
                        var exc = new UnknownResourceFormatException();
                        if ( errorHandler != null )
                        {
                            errorHandler( new FilesSearchContext( rootDir.FullName ), exc );
                            return;
                        }
                        throw exc;
                    }

                    ProcessResourceFile( 
                        new FilesSearchContext( filePattern ),
                        () => new ResourceFile( format ), action, errorHandler );
                }
                else
                {
                    var exc = new FileNotFoundException();
                    if ( errorHandler != null )
                    {
                        errorHandler( new FilesSearchContext( filePattern ), exc );
                        return;
                    }
                    throw exc;
                }
                return;
            }

            var filesProcessed = 0;
            var filesFailed = 0;
            var mask = Path.GetFileName( filePattern );
            foreach ( var file in Directory.EnumerateFiles( rootDir.FullName, mask, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly ) )
            {
                if ( ProcessResourceFile(
                    new FilesSearchContext( file, filesProcessed, filesFailed ),
                    () => new ResourceFile( file ), action, errorHandler ) )
                {
                    filesProcessed++;
                }
                else
                {
                    filesFailed++;
                }
            }

            if ( filesProcessed + filesFailed == 0 )
            {
                var exc = new FileNotFoundException();
                if ( errorHandler != null )
                {
                    errorHandler( new FilesSearchContext( filePattern, filesProcessed, filesFailed ), exc );
                    return;
                }
                throw exc;
            }
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