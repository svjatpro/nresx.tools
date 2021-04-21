using CommandLine;
using nresx.Tools;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb("format", HelpText = "format resource texts")]
    public class FormatCommand : ICommand
    {
        [Option( 's', "source", HelpText = "Source resource file" )]
        public string Source { get; set; }

        [Option( 'd', "destination", HelpText = "Destination resource file" )]
        public string Destination { get; set; }

        [Option( 'f', "format", HelpText = "New resource format" )]
        public string Format { get; set; }


        
        [Option( 'p', "pattern", HelpText = "Pattern to apply to all texts", Required = false )]
        public string Pattern { get; set; }

        [Option( "start-with", HelpText = "Add or remove prefix to all texts", Required = false )]
        public bool StartWith { get; set; }

        [Option( "end-with", HelpText = "Add or remove postfix to all texts", Required = false )]
        public bool EndWith { get; set; }

        [Option( 'l', "language-code", HelpText = "Use language code as prefix or postfix", Required = false )]
        public bool LanguageCode { get; set; }

        [Option( 'c', "culture-code", HelpText = "Use culture code as prefix or postfix", Required = false )]
        public bool CultureCode { get; set; }

        [Option( 'r', "remove", HelpText = "Remove prefix or postfix", Required = false)]
        public bool Remove { get; set; }

        public void Execute()
        {
            // format single resource file entries
            if ( !string.IsNullOrWhiteSpace( Source ) )
            {
                //    ResourceFormatType format;
                //    if ( !string.IsNullOrWhiteSpace( Format ) && 
                //         OptionHelper.DetectResourceFormat( Format, out var f1 ) )
                //    {
                //        format = f1;
                //    }
                //    else if ( !string.IsNullOrWhiteSpace( Destination ) &&
                //              ResourceFormatHelper.DetectFormatByExtension( Destination, out var f2 ) )
                //    {
                //        format = f2;
                //    }
                //    else
                //    {
                //        throw new ArgumentNullException();
                //    }

                //    if ( string.IsNullOrWhiteSpace( Destination ) )
                //    { 
                //        if( ResourceFormatHelper.DetectExtension( format, out var ext ) )
                //        {
                //            Destination = Path.ChangeExtension( Source, ext );
                //        }
                //        else
                //        {
                //            throw new ArgumentNullException();
                //        }
                //    }

                //    Console.WriteLine( $"d: {Destination}  f: {Format}");

                var res = new ResourceFile( Source );
                if ( Remove ) // remove
                {
                    if ( StartWith )
                        res.RemovePrefix( Pattern );
                    else if ( EndWith )
                        res.RemovePostfix( Pattern );
                }
                else // append
                {
                    if ( StartWith )
                        res.AddPrefix( Pattern );
                    else if ( EndWith )
                        res.AddPostfix( Pattern );
                }
                res.Save( Source );

                //res.Save( Destination, format );
            }

            //Console.WriteLine( $"executing convert {Source}" ); // 
        }
    }
}