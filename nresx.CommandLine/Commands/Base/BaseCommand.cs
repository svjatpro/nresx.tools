using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CommandLine;
using nresx.Tools;
using nresx.Tools.Exceptions;
using nresx.Tools.Extensions;
using nresx.Tools.Helpers;

namespace nresx.CommandLine.Commands
{
    public abstract class BaseCommand : ICommand
    {
        #region private fields

        protected const string FilesNotFoundErrorMessage = "fatal: path mask '{0}' did not match any files";
        protected const string FileLoadErrorMessage = "fatal: invalid file: '{0}' can't load resource file";
        protected const string DirectoryNotFoundErrorMessage = "fatal: Invalid path: '{0}': no such file or directory";
        protected const string FormatUndefinedErrorMessage = "fatal: resource format is not defined";
        protected const string FileAlreadyExistErrorMessage = "fatal: file '{0}' already exist";

        #endregion

        #region Common options

        [Option( 's', "source", HelpText = "Source resource file(s)" )]
        public IEnumerable<string> SourceFiles { get; set; }

        [Value( 0, Hidden = true )]
        public IEnumerable<string> Args { get; set; }


        [Option( 'r', "recursive", HelpText = "Search source files recursively" )]
        public bool Recursive { get; set; }
        protected virtual bool IsRecursiveAllowed => false;

        [Option( "new-file", HelpText = "Create new file, if it not exists" )]
        public bool CreateNewFile { get; set; }
        protected virtual bool IsCreateNewFileAllowed => false;

        [Option( "new-element", HelpText = "Create new element, if it not exists" )]
        public bool CreateNewElement { get; set; }
        protected virtual bool IsCreateNewElementAllowed => false;
        
        [Option( 'f', "format", HelpText = "New resource format" )]
        public string Format { get; set; }
        protected virtual bool IsFormatAllowed => false;
        

        [Option( "dry-run", HelpText = "Test command without actual performing", Hidden = true)]
        public bool DryRun { get; set; }
        protected virtual bool IsDryRunAllowed => true;

        #endregion

        public abstract void Execute();

        public bool Successful { get; protected set; } = true;
        public Exception Exception { get; protected set; } = null;

        protected OptionContext Options()
        {
            return new OptionContext( Args.ToList(), true );
        }

        protected void ForEachResourceGroup(
            List<string> sourceFiles,
            Action<GroupSearchContext, List<ResourceFile>> resourceAction,
            Action<GroupSearchContext, Exception> errorHandler = null,
            bool splitFiles = false )
        {
            if ( sourceFiles?.Count > 0 )
            {
                var groups = new List<List<FileInfo>>();

                var resFiles = new List<(FilesSearchContext context, ResourceFormatType format, CultureInfo culture)>();

                // query all resource files
                for ( var i = 0; i < sourceFiles.Count; i++ )
                {
                    var sourcePattern = sourceFiles[i];
                    FilesHelper.SearchFiles( 
                        sourcePattern, 
                        context =>
                        {
                            if ( context.FileExists &&
                               ( ResourceFormatHelper.DetectFormatByExtension( context.FullName, out var format ) ||
                                 context.SourcePathSpec.IsRegularName() ) )
                            {
                                context.FullName.TryToExtractCultureFromPath( out var culture );
                                resFiles.Add( (context, format, culture) );
                            }
                        },
                        recursive: Recursive && IsRecursiveAllowed,
                        createNew: CreateNewFile && IsCreateNewFileAllowed,
                        dryRun: DryRun && IsDryRunAllowed );
                }

                // try to get group of resource in the same folder
                resFiles = resFiles
                    .GroupBy( r => Path.GetDirectoryName( r.context.FullName ) )
                    .SelectMany( grp =>
                    {
                        var notProcessed = new List<(FilesSearchContext context, ResourceFormatType format, CultureInfo culture)>();
                        grp
                            .GroupBy( f => f.format )
                            .ToList()
                            .ForEach( g =>
                            {
                                var candidates = g.Where( f => f.format != ResourceFormatType.NA && f.culture != null ).ToList();
                                if ( candidates.Count > 1 )
                                {
                                    groups.Add( candidates.Select( f => f.context.CurrentFile ).ToList() );
                                    notProcessed.AddRange( g.Where( f => f.culture == null || f.format == ResourceFormatType.NA ) );
                                }
                                else
                                {
                                    notProcessed.AddRange( g );
                                }
                            });
                        return notProcessed;
                    } )
                    .ToList();

                // try to get group from culture specific folders
                if ( resFiles.Any() )
                {
                    resFiles = resFiles
                        .GroupBy( r => Path.GetFullPath( Path.Combine( r.context.FullName, "..", ".." ) ) )
                        .SelectMany( grp =>
                        {
                            var notProcessed = new List<(FilesSearchContext context, ResourceFormatType format, CultureInfo culture)>();
                            grp
                                .GroupBy( f => f.format )
                                .ToList()
                                .ForEach( g =>
                                {
                                    var candidates = g.Where( f => f.format != ResourceFormatType.NA && f.culture != null ).ToList();
                                    if ( candidates.Count > 1 )
                                    {
                                        groups.Add( candidates.Select( f => f.context.CurrentFile ).ToList() );
                                        notProcessed.AddRange( g.Where( f => f.culture == null || f.format == ResourceFormatType.NA ) );
                                    }
                                    else
                                    {
                                        notProcessed.AddRange( g );
                                    }
                                } );
                            return notProcessed;
                        } )
                        .ToList();
                }

                // all remain resources add as a separate groups
                if ( resFiles.Any() )
                {
                    groups.AddRange( resFiles.Select( f => new List<FileInfo>{ f.context.CurrentFile } ).ToList() );
                }

                groups.ForEach( group =>
                {
                    var files = group.Select( g => new ResourceFile( g.FullName ) ).ToList();
                    var context = new GroupSearchContext( groups.Count, groups.Select( g => g.Count ).Sum() );
                    resourceAction( context, files );
                } );
            }
        }

        protected void ForEachSourceFile(
            List<string> sourceFiles, 
            Action<FilesSearchContext, ResourceFile> resourceAction,
            Action<FilesSearchContext, Exception> errorHandler = null,
            bool splitFiles = false)
        {
            if ( sourceFiles?.Count > 0 )
            {
                for ( var i = 0; i < sourceFiles.Count; i++ )
                {
                    var sourcePattern = sourceFiles[i];
                    if( i > 0 && splitFiles ) Console.WriteLine( new string( '-', 30 ) );
                    FilesHelper.SearchResourceFiles(
                        sourcePattern,
                        resourceAction,
                        errorHandler ??
                        ( ( context, exception ) =>
                        {
                            if ( ( context.FilesProcessed + context.FilesFailed ) > 0 )
                                Console.WriteLine( new string( '-', 30 ) );

                            switch ( exception )
                            {
                                case FileNotFoundException:
                                    Console.WriteLine( FilesNotFoundErrorMessage, sourcePattern );
                                    break;
                                case DirectoryNotFoundException:
                                    Console.WriteLine( DirectoryNotFoundErrorMessage, sourcePattern );
                                    break;
                                case UnknownResourceFormatException:
                                    Console.WriteLine( FormatUndefinedErrorMessage, sourcePattern );
                                    break;
                                case FileLoadException:
                                default:
                                    Console.WriteLine( FileLoadErrorMessage, context.FullName );
                                    break;
                            }
                        } ),
                        recursive: Recursive && IsRecursiveAllowed,
                        createNew: CreateNewFile && IsCreateNewFileAllowed,
                        dryRun: DryRun && IsDryRunAllowed,
                        formatOption: IsFormatAllowed ? Format.ToExtension() : null );
                }
            }
        }

        protected bool TryOpenResourceFile( string path, out ResourceFile resourceFile, bool createNonExisting = false )
        {
            if ( string.IsNullOrWhiteSpace( path ) || ( !new FileInfo( path ).Exists && !createNonExisting ) )
            {
                Console.WriteLine( FilesNotFoundErrorMessage, path );
                resourceFile = null;
                return false;
            }

            try
            {
                resourceFile = 
                    ( !new FileInfo( path ).Exists && createNonExisting ) ?
                    new ResourceFile( ResourceFormatHelper.GetFormatType( path ) ) :
                    new ResourceFile( path );
                return true;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine( FilesNotFoundErrorMessage, path );
            }
            catch ( FileLoadException )
            {
                Console.WriteLine( FileLoadErrorMessage, path );
            }

            resourceFile = null;
            return false;
        }
    }
}