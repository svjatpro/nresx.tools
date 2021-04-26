using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using nresx.Tools;

namespace nresx.CommandLine.Commands
{
    [Verb( "info", HelpText = "get resource info" )]
    public class InfoCommand : ICommand
    {
        [Option( 's', "source", HelpText = "Source resource file" )]
        public IEnumerable<string> SourceFiles { get; set; }

        [Value( 0 )]
        public IEnumerable<string> SourceFilesValues { get; set; }

        public void Execute()
        {
            var files = new List<string>();
            if( SourceFilesValues?.Count() > 0 )
                files.AddRange( SourceFilesValues );
            if( SourceFiles.Any() )
                files.AddRange( SourceFiles );

            if ( files.Any() )
            {
                foreach ( var file in files )
                {
                    try
                    {
                        var res = new ResourceFile( file );
                        Console.WriteLine( $"Resource file name: \"{res.Name}\", (\"{res.AbsolutePath})\"" );
                        Console.WriteLine( $"resource format type: {res.ResourceFormat}" );
                        Console.WriteLine( $"text elements: {res.Elements.Count()}" );
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