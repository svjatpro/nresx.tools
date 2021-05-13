using System;
using System.IO;
using CommandLine;
using nresx.Tools;
using nresx.Tools.Helpers;

namespace nresx.CommandLine.Commands
{
    [Verb("convert", HelpText = "convert to another format")]
    public class ConvertCommand : BaseCommand, ICommand
    {
        public void Execute()
        {
            GetSourceDestinationPair( out var source, out var destination );

            // convert single resource file
            if ( !string.IsNullOrWhiteSpace( source ) )
            {
                ResourceFormatType format;
                if ( !string.IsNullOrWhiteSpace( Format ) && 
                     OptionHelper.DetectResourceFormat( Format, out var f1 ) )
                {
                    format = f1;
                }
                else if ( !string.IsNullOrWhiteSpace( destination ) &&
                          ResourceFormatHelper.DetectFormatByExtension( destination, out var f2 ) )
                {
                    format = f2;
                }
                else
                {
                    throw new ArgumentNullException();
                }
                
                if ( string.IsNullOrWhiteSpace( destination ) )
                { 
                    if( ResourceFormatHelper.DetectExtension( format, out var ext ) )
                    {
                        destination = Path.ChangeExtension( source, ext );
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }
                }

                Console.WriteLine( $"d: {destination}  f: {Format}");

                var res = new ResourceFile( source );
                res.Save( destination, format );
            }
            
            Console.WriteLine( $"executing convert {source}" ); // 
        }
    }
}