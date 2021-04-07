using System;
using CommandLine;

namespace NResx.Tools.CommandLine.Commands
{
    [Verb("convert", HelpText = "convert to another format")]
    public class ConvertCommand : ICommand
    {
        [Option( 's', "source", HelpText = "Source resource file" )]
        public string Source { get; set; }
        
        [Option( 'd', "destination", HelpText = "Destination resource file" )]
        public string Destination { get; set; }

        [Option( 'f', "format", HelpText = "New resource format" )]
        public string Format { get; set; }

        public void Execute()
        {
            if ( !string.IsNullOrWhiteSpace( Source ) &&
                 !string.IsNullOrWhiteSpace( Destination ) )
            {
                var format = ResourceFormatType.NA;
                if ( !string.IsNullOrWhiteSpace( Format ) && OptionHelper.DetectResourceFormat( Format, out var f ) )
                    format = f;
                // if( format == NA ) // detect format by destination extension

                var res = new ResourceFile( Source );
                res.Save( Destination, format );
            }
            
            Console.WriteLine( $"executing convert {Source}" ); // 
        }
    }
}