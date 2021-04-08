using System;
using System.IO;
using CommandLine;
using nresx.Tools;
using nresx.Tools.Helpers;

namespace nresx.CommandLine.Commands
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
            // convert single resource file
            if ( !string.IsNullOrWhiteSpace( Source ) )
            {
                ResourceFormatType format;
                if ( !string.IsNullOrWhiteSpace( Format ) && 
                     OptionHelper.DetectResourceFormat( Format, out var f1 ) )
                {
                    format = f1;
                }
                else if ( !string.IsNullOrWhiteSpace( Destination ) &&
                          ResourceFormatHelper.DetectFormatByExtension( Destination, out var f2 ) )
                {
                    format = f2;
                }
                else
                {
                    throw new ArgumentNullException();
                }
                
                if ( string.IsNullOrWhiteSpace( Destination ) )
                { 
                    if( ResourceFormatHelper.DetectExtension( format, out var ext ) )
                    {
                        Destination = Path.ChangeExtension( Source, ext );
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }
                }

                Console.WriteLine( $"d: {Destination}  f: {Format}");

                var res = new ResourceFile( Source );
                res.Save( Destination, format );
            }
            
            Console.WriteLine( $"executing convert {Source}" ); // 
        }
    }
}