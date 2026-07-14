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
            "fatal: path mask '{0}' did not match any files. Check the path is correct.";
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
        protected const string UnknownOutputFormatErrorMessage =
            "Unknown output format: '{0}'. Supported: text, json";
        protected const string RecursiveHintMessage =
            "did you mean to add -r to search subdirectories?";

        #endregion

        // Default search-error reporting shared by the ForEach* helpers: the standard
        // fatal message per exception type, plus an actionable -r hint when a wildcard
        // search matched nothing but subdirectories exist (RSX-233, issue #3).
        protected void WriteSearchError( FilesSearchContext context, Exception exception, string sourcePattern )
        {
            switch ( exception )
            {
                case FileNotFoundException:
                    WriteError( ExitNotFound, FilesNotFoundErrorMessage, sourcePattern );
                    WriteRecursiveHintIfUseful( sourcePattern );
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
        }

        // The hint must be actionable: only for wildcard pathspecs (-r does not search
        // subdirectories for an exact file name), only when -r wasn't already given, the
        // command supports it, and the searched directory actually has subdirectories.
        private void WriteRecursiveHintIfUseful( string sourcePattern )
        {
            if ( Recursive || !IsRecursiveAllowed || sourcePattern.IsRegularName() )
                return;

            var dir = Path.GetDirectoryName( sourcePattern );
            var root = string.IsNullOrWhiteSpace( dir ) ? Environment.CurrentDirectory : dir;
            try
            {
                if ( Directory.Exists( root ) && Directory.EnumerateDirectories( root ).Any() )
                    Console.Error.WriteLine( RecursiveHintMessage );
            }
            catch
            {
                // the hint is best-effort - never fail the command over it
            }
        }

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

        // Expands the source patterns into concrete files (CLI-side wildcard handling stays here,
        // RSX-128) then delegates the multi-file grouping and base-language pick to
        // nresx.Core's ResourceGroup.Detect (RSX-235) so the library and CLI share one implementation.
        protected void ForEachResourceGroup(
            List<string> sourceFiles,
            Action<GroupSearchContext, ResourceGroup> resourceAction,
            string baseLanguage = null,
            Action<string> onFileLoading = null,
            Func<string, Exception, bool> onLoadError = null )
        {
            if ( sourceFiles?.Count > 0 )
            {
                var paths = new List<string>();

                // query all resource files; route search errors to stderr + exit code the
                // same way ForEachSourceFile does - without a handler a bad mask or missing
                // directory crashed the process with an unhandled exception (RSX-248).
                for ( var i = 0; i < sourceFiles.Count; i++ )
                {
                    var sourcePattern = sourceFiles[i];
                    FilesHelper.SearchFiles(
                        sourcePattern,
                        context =>
                        {
                            if ( !context.FileExists )
                            {
                                // an explicitly named file that does not exist is an error;
                                // a mask that matched nothing is reported by SearchFiles itself
                                if ( context.SourcePathSpec.IsRegularName() )
                                    WriteError( ExitNotFound, FilesNotFoundErrorMessage, context.SourcePathSpec );
                                return;
                            }

                            if ( ResourceFormatHelper.DetectFormatByExtension( context.FullName, out _ ) )
                            {
                                paths.Add( context.FullName );
                            }
                            else if ( context.SourcePathSpec.IsRegularName() )
                            {
                                // explicitly named file in a format the registry does not know:
                                // report it here - passing it on would crash ResourceGroup.Detect
                                WriteError( ExitFormatError, FormatUndefinedErrorMessage );
                            }
                        },
                        ( context, exception ) => WriteSearchError( context, exception, sourcePattern ),
                        recursive: Recursive && IsRecursiveAllowed,
                        createNew: CreateNewFile && IsCreateNewFileAllowed,
                        dryRun: DryRun && IsDryRunAllowed );
                }

                var groups = ResourceGroup.Detect( paths, baseLanguage, onFileLoading, onLoadError );
                var totalFiles = groups.Sum( g => g.Files.Count );
                foreach ( var group in groups )
                {
                    var context = new GroupSearchContext( groups.Count, totalFiles );
                    resourceAction( context, group );
                }
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

                            WriteSearchError( context, exception, sourcePattern );
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

                            WriteSearchError( context, exception, sourcePattern );
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