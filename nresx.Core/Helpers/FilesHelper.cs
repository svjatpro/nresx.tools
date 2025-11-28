using System;
using System.IO;
using nresx.Tools.Exceptions;
using nresx.Tools.Extensions;

namespace nresx.Tools.Helpers
{
    public class GroupSearchContext
    {
        //public readonly string SourcePathSpec;
        //public readonly string PathSpec;
        /*public readonly FileInfo CurrentFile;
        public readonly int FilesProcessed;
        public readonly int FilesFailed;*/

        public readonly int TotalGroups;
        public readonly int TotalResourceFiles;

        public GroupSearchContext( int totalGroups, int totalResourceFiles )
        {
            TotalGroups = totalGroups;
            TotalResourceFiles = totalResourceFiles;

            //SourcePathSpec = sourcePathSpec;
            //PathSpec = pathSpec;
            //if ( PathSpec.IsRegularName() )
            //    CurrentFile = new FileInfo( pathSpec );
            //FilesProcessed = filesProcessed;
            //FilesFailed = filesFailed;
        }

        //public string FileName => CurrentFile?.Name ?? Path.GetFileName( PathSpec );
        //public string FullName => CurrentFile?.FullName ?? PathSpec;
        //public bool FileExists => CurrentFile?.Exists ?? false;
    }

    public class FilesSearchContext
    {
        public readonly string SourcePathSpec;
        public readonly string PathSpec;
        public readonly FileInfo CurrentFile;
        public readonly int FilesProcessed;
        public readonly int FilesFailed;

        public FilesSearchContext( string sourcePathSpec, string pathSpec, int filesProcessed = 0, int filesFailed = 0 )
        {
            SourcePathSpec = sourcePathSpec;
            PathSpec = pathSpec;
            if ( PathSpec.IsRegularName() ) 
                CurrentFile = new FileInfo( pathSpec );
            FilesProcessed = filesProcessed;
            FilesFailed = filesFailed;
        }

        public string FileName => CurrentFile?.Name ?? Path.GetFileName( PathSpec);
        public string FullName => CurrentFile?.FullName ?? PathSpec;
        public bool FileExists => CurrentFile?.Exists ?? false;
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
            SearchFiles(
                filePattern,
                ( context ) =>
                {
                    if ( context.FullName.IsRegularName() && !context.FileExists )
                    {
                        if ( createNew )
                        {
                            ResourceFormatType format;
                            if ( ResourceFormatHelper.DetectFormatByExtension( formatOption, out var formatByOption ) )
                            {
                                format = formatByOption;
                            }
                            else if ( ResourceFormatHelper.DetectFormatByExtension( filePattern,
                                out var formatByExtension ) )
                            {
                                format = formatByExtension;
                            }
                            else
                            {
                                var exc = new UnknownResourceFormatException();
                                if ( errorHandler != null )
                                {
                                    errorHandler( context, exc );
                                    return;
                                }
                                throw exc;
                            }

                            action( context, new ResourceFile( format ) );
                        }
                        else
                        {
                            var exc = new FileNotFoundException();
                            if ( errorHandler != null )
                            {
                                errorHandler( context, exc );
                                return;
                            }

                            throw exc;
                        }
                        return;
                    }
                    
                    var resource = new ResourceFile( context.FullName );
                    action( context, resource );
                }, 
                (context, exception) =>
                {
                    // skip error message for non resource files, if search by spec
                    if ( !context.SourcePathSpec.IsRegularName() &&
                         ( exception is UnknownResourceFormatException ||
                           exception is FileLoadException ) )
                    {
                        return;
                    }

                    if ( errorHandler != null )
                    {
                        errorHandler( context, exception );
                    }
                },
                recursive, createNew, dryRun );
        }

        public static void SearchFiles( string filePattern,
            Action<FilesSearchContext> action,
            Action<FilesSearchContext, Exception> errorHandler = null,
            bool recursive = false,
            bool createNew = false,
            bool dryRun = false )
        {
            var isFileName = filePattern.IsRegularName();
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
                        errorHandler( new FilesSearchContext( filePattern, rootDir.FullName ), exc );
                        return;
                    }
                    throw exc;
                }
            }

            if ( isFileName && !new FileInfo( filePattern ).Exists )
            {
                action( new FilesSearchContext( filePattern, filePattern ) );
                return;
            }

            var filesProcessed = 0;
            var filesFailed = 0;
            var mask = Path.GetFileName( filePattern );
            foreach ( var file in Directory.EnumerateFiles( rootDir.FullName, mask, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly ) )
            {
                var context = new FilesSearchContext( filePattern, file, filesProcessed, filesFailed );
                try
                {
                    action( context );
                    filesProcessed++;
                }
                catch ( Exception ex )
                {
                    if ( errorHandler != null )
                    {
                        errorHandler( context, ex );
                        filesFailed++;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if ( filesProcessed + filesFailed == 0 )
            {
                var exc = new FileNotFoundException();
                if ( errorHandler != null )
                {
                    errorHandler( new FilesSearchContext( filePattern, filePattern, filesProcessed, filesFailed ), exc );
                    return;
                }
                throw exc;
            }
        }

        public static void CopyDirectory( string sourceDir, string destDir )
        {
            var dir = new DirectoryInfo( sourceDir );

            if ( !dir.Exists ) throw new DirectoryNotFoundException( sourceDir );

            var dirs = dir.GetDirectories();
            Directory.CreateDirectory( destDir );

            var files = dir.GetFiles();
            foreach ( var file in files )
            {
                var tempPath = Path.Combine( destDir, file.Name );
                file.CopyTo( tempPath, false );
            }

            foreach ( var subdir in dirs )
            {
                var tempPath = Path.Combine( destDir, subdir.Name );
                CopyDirectory( subdir.FullName, tempPath );
            }
        }
    }
}