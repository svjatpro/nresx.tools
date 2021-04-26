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
    [Verb("convert", HelpText = "convert to another format")]
    public class ConvertCommand : BaseCommand, ICommand
    {
        [Value( 0, Hidden = true )]
        public IEnumerable<string> Args { get; set; }
        
        public void Execute()
        {
            var args = Args?.ToList() ?? new List<string>();
            var source = Source ?? args.Take();
            var destination = Destination ?? args.Take();

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