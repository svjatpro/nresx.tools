using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using nresx.Tools.Helpers;

namespace nresx.CommandLine.Commands
{
    [Verb( "info", HelpText = "get resource info" )]
    public class InfoCommand : ICommand
    {
        [Option( 's', "source", HelpText = "Source resource file" )]
        public IEnumerable<string> SourceFiles { get; set; }

        [Value( 0 )]
        public IEnumerable<string> SourceFilesValues { get; set; }

        [Option( 'r', "recursive", HelpText = "Recursively" )]
        public bool Recursive { get; set; }

        public void Execute()
        {
            var filesPatterns = new List<string>();
            if( SourceFilesValues?.Count() > 0 )
                filesPatterns.AddRange( SourceFilesValues );
            if( SourceFiles.Any() )
                filesPatterns.AddRange( SourceFiles );
            
            if ( filesPatterns.Any() )
            {
                foreach ( var filePattern in filesPatterns )
                {
                    try
                    {
                        FilesHelper.SearchResourceFiles(
                            filePattern,
                            fileInfo =>
                            {
                                Console.WriteLine(
                                    $"Resource file name: \"{fileInfo.Name}\", (\"{fileInfo.AbsolutePath})\"" );
                                Console.WriteLine( $"resource format type: {fileInfo.ResourceFormat}" );
                                Console.WriteLine( $"text elements: {fileInfo.Elements.Count()}" );
                                Console.WriteLine( new string( '-', 30 ) );
                            },
                            errorHandler: ( fileInfo, exception ) =>
                            {
                                switch( exception )
                                {
                                    case FileNotFoundException:
                                        Console.WriteLine( $"fatal: path mask '{filePattern}' did not match any files" );
                                        break;
                                    case FileLoadException:
                                    default:
                                        Console.WriteLine( $"fatal: invalid file: '{fileInfo.FullName}' can't load resource file" );
                                        break;
                                }
                                Console.WriteLine( new string( '-', 30 ) );
                            },
                            recursive: Recursive );
                    }
                    catch ( FileNotFoundException ex )
                    {
                        Console.WriteLine( $"fatal: path mask '{filePattern}' did not match any files" );
                        Console.WriteLine( new string( '-', 30 ) );
                    }
                    catch ( DirectoryNotFoundException ex )
                    {
                        Console.WriteLine( $"fatal: Invalid path: '{filePattern}': no such file or directory" );
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