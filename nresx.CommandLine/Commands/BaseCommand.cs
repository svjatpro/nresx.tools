using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using nresx.Tools;
using nresx.Tools.Exceptions;
using nresx.Tools.Extensions;
using nresx.Tools.Helpers;

namespace nresx.CommandLine.Commands
{
    public class OptionContext
    {
        public readonly List<string> FreeArgs;
        public readonly bool Success;
        public readonly List<string> Errors;
        
        public OptionContext( List<string> freeArgs, bool hasErrors, List<string> errors = null )
        {
            FreeArgs = freeArgs;
            Success = hasErrors;
            Errors = errors ?? new List<string>();
        }
    }
    public static class CommandExtensions
    {
        private const string MissingOptionMessage = "Required option '{0}' is missing.";

        public static OptionContext Multiple( 
            this OptionContext context, 
            IEnumerable<string> mappedValues, 
            out List<string> values, 
            bool mandatory = false, 
            string optionName = null )
        {
            var src = mappedValues?.ToList();
            var args = context.FreeArgs?.ToList();
            var errors = context.Errors;
            values = new List<string>();

            if ( src?.Count > 0 )
            {
                values.AddRange( src );
            }
            else if( args.TryTake( out var val ) )
            {
                values.Add( val );
            }

            var success = !mandatory || values?.Count > 0;
            if ( !success && optionName != null )
                errors.Add( string.Format( MissingOptionMessage, optionName ) );

            return new OptionContext( args, success, errors );
        }

        public static OptionContext Single( 
            this OptionContext context, 
            string mappedValue, 
            out string value, 
            bool mandatory = false,
            string optionName = null )
        {
            var args = context.FreeArgs?.ToList();
            var errors = context.Errors;

            if ( !string.IsNullOrWhiteSpace( mappedValue ) )
            {
                value = mappedValue;
            }
            else if ( args.TryTake( out var val ) )
            {
                value = val;
            }
            else
            {
                value = null;
            }

            var success = !mandatory || !string.IsNullOrWhiteSpace( value );
            if ( !success && optionName != null )
                errors.Add( string.Format( MissingOptionMessage, optionName ) );

            return new OptionContext( args, success, errors );
        }

        public static bool Validate( this OptionContext context )
        {
            if ( !context.Success )
            {
                foreach ( var error in context.Errors )
                {
                    Console.WriteLine( error );
                }
            }
            return context.Success;
        }
    }

    public abstract class BaseCommand : ICommand
    {
        #region private fields

        protected const string FilesNotFoundErrorMessage = "fatal: path mask '{0}' did not match any files";
        protected const string FileLoadErrorMessage = "fatal: invalid file: '{0}' can't load resource file";
        protected const string DirectoryNotFoundErrorMessage = "fatal: Invalid path: '{0}': no such file or directory";
        protected const string FormatUndefinedErrorMessage = "fatal: resource format is not defined";

        #endregion

        #region Common options

        [Option( 's', "source", HelpText = "Source resource file(s)" )]
        public IEnumerable<string> SourceFiles { get; set; }

        [Option( 'd', "destination", HelpText = "Destination resource file" )]
        public string Destination { get; set; }

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



        [Option( "dry-run", HelpText = "Test remove command without actual performing" )]
        public bool DryRun { get; set; }
        protected virtual bool IsDryRunAllowed => true;

        #endregion

        public abstract void Execute();

        public bool Successful { get; protected set; } = true;
        public Exception Exception { get; protected set; } = null;

        protected List<string> GetSourceFiles()
        {
            var src = SourceFiles?.ToList();
            if ( src?.Count > 0 )
                return src;

            var args = Args?.ToList();
            if ( args?.Count > 0 )
                return args;

            return new List<string>();
        }
        protected void GetSourceDestinationPair( out string source, out string destination )
        {
            var args = Args?.ToList() ?? new List<string>();
            source = SourceFiles?.FirstOrDefault() ?? args.Take();
            destination = SourceFiles?.Skip( 1 ).FirstOrDefault() ?? Destination ?? args.Take();
        }

        protected OptionContext Options()
        {
            return new OptionContext( Args.ToList(), true );
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
                    //try
                    //{
                    if( i > 0 && splitFiles ) Console.WriteLine( new string( '-', 30 ) );
                    FilesHelper.SearchResourceFiles(
                        sourcePattern,
                        resourceAction,
                        errorHandler ??
                        ( ( context, exception ) =>
                        {
                            //if ( Interlocked.Read( ref filesProcessed ) > 0 )
                            if ( ( context.FilesProcessed + context.FilesFaled ) > 0 )
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
                                    Console.WriteLine( FileLoadErrorMessage, context.CurrentFile.FullName );
                                    break;
                            }
                        } ),
                        recursive: Recursive && IsRecursiveAllowed,
                        createNew: CreateNewFile && IsCreateNewFileAllowed,
                        dryRun: DryRun && IsDryRunAllowed,
                        formatOption: IsFormatAllowed ? Format.ToExtension() : null );
                    //}
                    //catch ( FileNotFoundException ex )
                    //{
                    //    if ( filesProcessed > 0 )
                    //        Console.WriteLine( new string( '-', 30 ) );
                    //    Console.WriteLine( FilesNotFoundErrorMessage, sourcePattern );
                    //    //Console.WriteLine( new string( '-', 30 ) ); // todo: replace with '====' split line, and show only for multiple files
                    //    Interlocked.Increment( ref filesProcessed );
                    //}
                    //catch ( DirectoryNotFoundException ex )
                    //{
                    //    if ( filesProcessed > 0 )
                    //        Console.WriteLine( new string( '-', 30 ) );
                    //    Console.WriteLine( DirectoryNotFoundErrorMessage, sourcePattern );
                    //    //Console.WriteLine( new string( '-', 30 ) ); // todo: replace with '====' split line, and show only for multiple files
                    //    Interlocked.Increment( ref filesProcessed );
                    //}
                    //catch ( UnknownResourceFormatException ex )
                    //{
                    //    if ( filesProcessed > 0 )
                    //        Console.WriteLine( new string( '-', 30 ) );
                    //    Console.WriteLine( FormatUndefinedErrorMessage, sourcePattern );
                    //    //Console.WriteLine( new string( '-', 30 ) ); // todo: replace with '====' split line, and show only for multiple files
                    //    Interlocked.Increment( ref filesProcessed );
                    //}
                    //catch ( Exception e )
                    //{
                    //    Console.ForegroundColor = ConsoleColor.Red;
                    //    if ( filesProcessed > 0 )
                    //        Console.WriteLine( new string( '-', 30 ) );
                    //    Console.WriteLine( e );
                    //    Console.ForegroundColor = ConsoleColor.Gray;
                    //    //Console.WriteLine( new string( '-', 30 ) ); // todo: replace with '====' split line, and show only for multiple files
                    //    Interlocked.Increment( ref filesProcessed );
                    //}
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