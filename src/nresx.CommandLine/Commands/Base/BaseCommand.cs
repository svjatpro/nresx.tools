using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CommandLine;
using nresx.CommandLine.Helpers;
using nresx.Core;
using nresx.Core.Exceptions;
using nresx.Core.Extensions;
using nresx.Core.Helpers;

namespace nresx.CommandLine.Commands
{
    public class ResourceFileInfo
    {
        public FileInfo FileInfo { get; }

        private ResourceFile _resource;
        public ResourceFile Resource
        {
            get
            {
                if ( _resource == null )
                    _resource = new ResourceFile( FileInfo.FullName );
                return _resource;
            }
        }

        public ResourceFileInfo( string path )
        {
            FileInfo = new FileInfo( path );
        }
    }
    public abstract class BaseCommand : ICommand
    {

        #region exit codes

        // Documented in src/nresx.CommandLine/README.md - keep that table in sync when adding codes.
        public const int ExitSuccess = 0;
        public const int ExitFailure = 1;             // general runtime error
        public const int ExitUsageError = 2;          // bad / missing arguments
        public const int ExitNotFound = 3;            // input file / directory / element not found
        public const int ExitFormatError = 4;         // format undefined or file failed to load
        public const int ExitDestinationConflict = 5; // destination already exists / required --new-file

        #endregion

        #region error message templates

        protected const string FilesNotFoundErrorMessage =
            "fatal: path mask '{0}' did not match any files. Check the path is correct, or use -r to search subdirectories.";
        protected const string FileLoadErrorMessage =
            "fatal: failed to load resource file '{0}'. The file may be corrupt or in an unrecognized format - pass -f <format> to override format detection.";
        protected const string DirectoryNotFoundErrorMessage =
            "fatal: path '{0}' does not exist. Check the path is correct.";
        protected const string FormatUndefinedErrorMessage =
            "fatal: resource format could not be determined. Pass -f <format> with one of: resx, resw, po, json, yaml, xliff, xml, strings, properties, arb, csv, tsv, xlsx, ini, txt.";
        protected const string FileAlreadyExistErrorMessage =
            "fatal: destination file '{0}' already exists. Move or rename the existing file before running this command.";
        protected const string ElementNotFoundErrorMessage =
            "fatal: element with key '{0}' was not found in '{1}'. Use 'nresx list' to see existing keys, or pass --new-element to add it.";
        protected const string UnknownCommandErrorMessage =
            "Unknown command: '{0}'";

        #endregion

        // Writes a fatal error to stderr and marks the command unsuccessful so the
        // process exits non-zero. The exit code argument categorizes the failure
        // for scripting (see RSX-151). Diagnostic / informational output stays on
        // stdout via Console.WriteLine.
        protected void WriteError( int exitCode, string format, params object[] args )
        {
            if ( args == null || args.Length == 0 )
                Console.Error.WriteLine( format );
            else
                Console.Error.WriteLine( format, args );
            Successful = false;
            if ( _exitCode == ExitSuccess )
                _exitCode = exitCode; // first error wins
        }

        // Marks the command as having a usage / argument error. Called by CommandExtensions.Validate
        // when option parsing helpers report missing required values.
        internal void MarkUsageError()
        {
            Successful = false;
            if ( _exitCode == ExitSuccess )
                _exitCode = ExitUsageError;
        }

        // Writes a diagnostic line to stdout, but only when --verbose is active.
        // Use for "what did the command actually do?" signals: matched files, format
        // detection, per-element operations, skipped destinations.
        protected void WriteVerbose( string format, params object[] args )
        {
            if ( !Verbose ) return;
            if ( args == null || args.Length == 0 )
                Console.WriteLine( "[verbose] " + format );
            else
                Console.WriteLine( "[verbose] " + format, args );
        }

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

        [Option( 'V', "verbose", HelpText = "Print extra diagnostics about file matching, format detection, and per-element actions" )]
        public bool Verbose { get; set; }

        [Option( "debug", HelpText = "Debug command", Hidden = true )]
        public bool Debugger { get; set; }

        #endregion

        protected CommandLineContext Context { get; private set; }
        public void SetContext( CommandLineContext context )
        {
            Context = context;
        }

        public virtual void Execute()
        {
#if DEBUG // available only in debug configuration (not for release)
            
            if ( Debugger ) // while this is --debug option for nresx command line
            {
                Console.WriteLine();
                Console.WriteLine( @"============== Debugger mode! ==============" );
                Console.WriteLine();

                System.Diagnostics.Debugger.Launch();
            }

#endif

            ExecuteCommand();
        }

        protected abstract void ExecuteCommand();

        // Real example invocations shown in the "Examples:" section of the detailed
        // per-command help (`nresx <command> --help` / `nresx help <command>`). Each
        // entry is one example - a leading '#' comment line (optional) plus the command
        // line, separated by '\n'. Lifted verbatim from src/nresx.CommandLine/README.md.
        protected virtual IEnumerable<string> HelpExamples => [];

        // Accessor so HelpRenderer (outside the inheritance chain) can read the
        // per-command examples while HelpExamples itself stays a protected override point.
        internal IEnumerable<string> GetHelpExamples() => HelpExamples ?? [];

        public bool Successful { get; protected set; } = true;
        public Exception Exception { get; protected set; } = null;

        private int _exitCode = ExitSuccess;
        public int ExitCode => Successful ? ExitSuccess : ( _exitCode == ExitSuccess ? ExitFailure : _exitCode );

        protected OptionContext Options()
        {
            return new OptionContext( Args.ToList(), true );
        }

        protected void ForEachResourceGroup(
            List<string> sourceFiles,
            Action<GroupSearchContext, List<ResourceFileInfo>> resourceAction,
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
                    //var files = group.Select( g => new ResourceFile( g.FullName ) ).ToList();
                    var files = group.Select( g => new ResourceFileInfo( g.FullName ) ).ToList();
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
                var recursive = Recursive && IsRecursiveAllowed;
                for ( var i = 0; i < sourceFiles.Count; i++ )
                {
                    var sourcePattern = sourceFiles[i];
                    if( i > 0 && splitFiles ) Console.WriteLine( new string( '-', 30 ) );
                    WriteVerbose( "searching '{0}'{1}", sourcePattern, recursive ? " (recursive)" : "" );
                    var wrappedAction = resourceAction;
                    if ( Verbose )
                    {
                        wrappedAction = ( ctx, res ) =>
                        {
                            WriteVerbose( "matched: {0} (format: {1})", ctx.FullName, res.FileFormat );
                            resourceAction( ctx, res );
                        };
                    }
                    FilesHelper.SearchResourceFiles(
                        sourcePattern,
                        wrappedAction,
                        errorHandler ??
                        ( ( context, exception ) =>
                        {
                            if ( ( context.FilesProcessed + context.FilesFailed ) > 0 )
                                Console.WriteLine( new string( '-', 30 ) );

                            switch ( exception )
                            {
                                case FileNotFoundException:
                                    WriteError( ExitNotFound, FilesNotFoundErrorMessage, sourcePattern );
                                    break;
                                case DirectoryNotFoundException:
                                    WriteError( ExitNotFound, DirectoryNotFoundErrorMessage, sourcePattern );
                                    break;
                                case UnknownResourceFormatException:
                                    WriteError( ExitFormatError, FormatUndefinedErrorMessage, sourcePattern );
                                    break;
                                case FileLoadException:
                                default:
                                    WriteError( ExitFormatError, FileLoadErrorMessage, context.FullName );
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

        protected void ForEachFile(
            List<string> sourceFiles,
            Action<FilesSearchContext> resourceAction,
            Action<FilesSearchContext, Exception> errorHandler = null,
            bool splitFiles = false )
        {
            if ( sourceFiles?.Count > 0 )
            {
                var recursive = Recursive && IsRecursiveAllowed;
                for ( var i = 0; i < sourceFiles.Count; i++ )
                {
                    var sourcePattern = sourceFiles[i];
                    if ( i > 0 && splitFiles ) Console.WriteLine( new string( '-', 30 ) );
                    WriteVerbose( "searching '{0}'{1}", sourcePattern, recursive ? " (recursive)" : "" );
                    var wrappedAction = resourceAction;
                    if ( Verbose )
                    {
                        wrappedAction = ctx =>
                        {
                            WriteVerbose( "matched: {0}", ctx.FullName );
                            resourceAction( ctx );
                        };
                    }
                    FilesHelper.SearchFiles(
                        sourcePattern,
                        wrappedAction,
                        errorHandler ??
                        ( ( context, exception ) =>
                        {
                            if ( ( context.FilesProcessed + context.FilesFailed ) > 0 )
                                Console.WriteLine( new string( '-', 30 ) );

                            switch ( exception )
                            {
                                case FileNotFoundException:
                                    WriteError( ExitNotFound, FilesNotFoundErrorMessage, sourcePattern );
                                    break;
                                case DirectoryNotFoundException:
                                    WriteError( ExitNotFound, DirectoryNotFoundErrorMessage, sourcePattern );
                                    break;
                                case UnknownResourceFormatException:
                                    WriteError( ExitFormatError, FormatUndefinedErrorMessage, sourcePattern );
                                    break;
                                case FileLoadException:
                                default:
                                    WriteError( ExitFormatError, FileLoadErrorMessage, context.FullName );
                                    break;
                            }
                        } ),
                        recursive: Recursive && IsRecursiveAllowed,
                        createNew: CreateNewFile && IsCreateNewFileAllowed,
                        dryRun: DryRun && IsDryRunAllowed );
                }
            }
        }

        protected bool TryOpenResourceFile( string path, out ResourceFile resourceFile, bool createNonExisting = false )
        {
            if ( string.IsNullOrWhiteSpace( path ) || ( !new FileInfo( path ).Exists && !createNonExisting ) )
            {
                WriteError( ExitNotFound, FilesNotFoundErrorMessage, path );
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
                WriteError( ExitNotFound, FilesNotFoundErrorMessage, path );
            }
            catch ( FileLoadException )
            {
                WriteError( ExitFormatError, FileLoadErrorMessage, path );
            }

            resourceFile = null;
            return false;
        }
    }
}