using System;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Tools;

namespace nresx.CommandLine.Commands
{
    [Verb( "list", HelpText = "get resource info" )]
    public class ListCommand : BaseCommand, ICommand
    {
        //[Option( 's', "source", HelpText = "Source resource file" )]
        //public string SourceFile { get; set; }

        //[Value( 0, HelpText = "Source resource file" )]
        //public string SourceFileValue { get; set; }

        [Option( 't', "template", HelpText = "Resource element output template.\n \\k - element key, \\v - element value, \\c - element comment" )]
        public string Template { get; set; }

        protected override void ExecuteCommand()
        {
            var optionsParsed = Options()
                .Single( SourceFiles, out var source, mandatory: true )
                .Validate();
            if ( !optionsParsed )
                return;

            //var source = SourceFile ?? SourceFileValue;
            var template = Template ?? "\\k: \\v";
            if ( !string.IsNullOrWhiteSpace( source ) )
            {
                var res = new ResourceFile( source );

                foreach ( var el in res.Elements )
                {
                    Console.WriteLine( template
                        .Replace( "\\k", el.Key ) 
                        .Replace( "\\v", el.Value ) 
                        .Replace( "\\c", el.Comment ) );
                }
            }
        }
    }
}