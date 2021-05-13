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
    public class BaseCommand
    {
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
                                        Console.WriteLine( $"fatal: path mask '{sourcePattern}' did not match any files" );
                                        break;
                                    case FileLoadException:
                                    default:
                                        Console.WriteLine( $"fatal: invalid file: '{fileInfo.FullName}' can't load resource file" );
                                        break;
                                }
                                Console.WriteLine( new string( '-', 30 ) );
                            }),
                            recursive: Recursive );
                    }
                    catch ( FileNotFoundException ex )
                    {
                        Console.WriteLine( $"fatal: path mask '{sourcePattern}' did not match any files" );
                        Console.WriteLine( new string( '-', 30 ) );
                    }
                    catch ( DirectoryNotFoundException ex )
                    {
                        Console.WriteLine( $"fatal: Invalid path: '{sourcePattern}': no such file or directory" );
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
    }
}