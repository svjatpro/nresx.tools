using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    var elements = ResourceFile.LoadRawElements( file );
                    var result = elements.ValidateElements( out var errors );
                    if ( result )
                    {
                        //
                    }
                    else
                    {
                        foreach ( var elementError in errors )
                        {
                            var msg = new StringBuilder();
                            msg.Append( $"{elementError.ErrorType}:" );
                            if ( !string.IsNullOrWhiteSpace( elementError.ElementKey ) )
                                msg.Append( $" {elementError.ElementKey};" );
                            if ( !string.IsNullOrWhiteSpace( elementError.Message ) )
                                msg.Append( $" {elementError.Message};" );
                            Console.WriteLine( msg.ToString() );
                        }
                    }

                }
            }
        }

        public bool Successful { get; } = true;
        public Exception Exception { get; } = null;
    }
}