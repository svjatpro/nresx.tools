using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using nresx.Tools;
using nresx.Tools.Extensions;
using nresx.Tools.Helpers;

namespace nresx.CommandLine.Commands
{
    public abstract class BaseCommand : ICommand
    {
        #region private fields

        protected const string FileNotFoundErrorMessage = "fatal: path mask '{0}' did not match any files";
        protected const string FileLoadErrorMessage = "fatal: invalid file: '{0}' can't load resource file";
        protected const string PathNotFoundErrorMessage = "fatal: Invalid path: '{0}': no such file or directory";

        #endregion

        #region Common options

        [Option( 's', "source", HelpText = "Source resource file(s)" )]
        public IEnumerable<string> SourceFiles { get; set; }

        [Option( 'd', "destination", HelpText = "Destination resource file" )]
        public string Destination { get; set; }

        //[Value( 0, HelpText = "Source resource file(s)" )]
        //public IEnumerable<string> SourceFilesValues { get; set; }

        [Value( 0, Hidden = true )]
        public IEnumerable<string> Args { get; set; }

        [Option( 'r', "recursive", HelpText = "Search source files recursively" )]
        public bool Recursive { get; set; }

        [Option( 'f', "format", HelpText = "New resource format" )]
        public string Format { get; set; }



        [Option( "dry-run", HelpText = "Test remove command without actual performing" )]
        public bool DryRun { get; set; }

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

        protected void ForEachSourceFile( List<string> sourceFiles, Action<ResourceFile> action, Action<FileInfo, Exception> errorHandler = null )
        {
            if ( sourceFiles?.Count > 0 )
            {
                foreach ( var sourcePattern in sourceFiles )
                {
                    try
                    {
                        FilesHelper.SearchResourceFiles(
                            sourcePattern, action,
                            errorHandler ?? 
                            (( fileInfo, exception ) =>
                            {
                                switch ( exception )
                                {
                                    case FileNotFoundException:
                                        Console.WriteLine( FileNotFoundErrorMessage, sourcePattern );
                                        break;
                                    case FileLoadException:
                                    default:
                                        Console.WriteLine( FileLoadErrorMessage, fileInfo.FullName );
                                        break;
                                }
                                Console.WriteLine( new string( '-', 30 ) );
                            }),
                            recursive: Recursive );
                    }
                    catch ( FileNotFoundException ex )
                    {
                        Console.WriteLine( FileNotFoundErrorMessage, sourcePattern );
                        Console.WriteLine( new string( '-', 30 ) );
                    }
                    catch ( DirectoryNotFoundException ex )
                    {
                        Console.WriteLine( PathNotFoundErrorMessage, sourcePattern );
                        Console.WriteLine( new string( '-', 30 ) );
                    }
                    catch ( Exception e )
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine( e );
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine( new string( '-', 30 ) );
                    }
                }
            }
        }

        protected bool TryOpenResourceFile( string path, out ResourceFile resourceFile, bool createNonExisting = false )
        {
            if ( string.IsNullOrWhiteSpace( path ) || ( !new FileInfo( path ).Exists && !createNonExisting ) )
            {
                Console.WriteLine( FileNotFoundErrorMessage, path );
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
                Console.WriteLine( FileNotFoundErrorMessage, path );
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