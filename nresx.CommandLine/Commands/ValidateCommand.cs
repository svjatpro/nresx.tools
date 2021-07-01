using System;
using System.Linq;
using System.Text;
using CommandLine;
using Microsoft.VisualBasic;
using nresx.CommandLine.Commands.Base;
using nresx.Tools;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "validate", HelpText = "Validate resource file(s)" )]
    public class ValidateCommand : BaseCommand
    {
        protected override bool IsRecursiveAllowed => true;

        public override void Execute()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true )
                .Validate();
            if ( !optionsParsed )
                return;
            
            ForEachSourceFile(
                sourceFiles,
                ( file, resource ) =>
                {
                    var elements = ResourceFile.LoadRawElements( file.FullName );
                    var result = elements.ValidateElements( out var errors );
                    if ( result )
                    {
                        //
                    }
                    else
                    {
                        if ( errors.Any() )
                        {
                            Console.WriteLine( $"Resource file: \"{resource.AbsolutePath}\"" );
                        }

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
                } );
        }
    }
}