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
                    if( i > 0 && splitFiles ) Console.WriteLine( new string( '-', 30 ) );
                    FilesHelper.SearchResourceFiles(
                        sourcePattern,
                        resourceAction,
                        errorHandler ??
                        ( ( context, exception ) =>
                        {
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