using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using nresx.Tools;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "validate", HelpText = "Validate resource file(s)" )]
    public class ValidateCommand : ICommand
    {
        [Option( 's', "source", HelpText = "Source resource file" )]
        public IEnumerable<string> SourceFiles { get; set; }
        [Value( 0 )]
        public IEnumerable<string> SourceFilesValues { get; set; }

        public void Execute()
        {
            //var optionsParsed = Options()
            //    .Multiple( SourceFiles, out var sourceFiles, mandatory: true )
            //    .Validate();
            //if ( !optionsParsed )
            //    return;


            var files = new List<string>();
            if( SourceFilesValues?.Count() > 0 )
                files.AddRange( SourceFilesValues );
            if( SourceFiles.Any() )
                files.AddRange( SourceFiles );

            if ( files.Any() )
            {
                foreach ( var file in files )
                {
                    var res = new ResourceFile( file );
                    var result = res.ValidateElements( out var errors );
                    if ( result )
                    {
                        //
                    }
                    else
                    {
                        foreach ( var elementError in errors )
                        {
                            Console.WriteLine( $"{elementError.ErrorType.ToString()}: {elementError.ElementKey}; {elementError.Message}" );
                        }
                    }

                }
            }
        }

        public bool Successful { get; } = true;
        public Exception Exception { get; } = null;
    }
}