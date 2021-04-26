using CommandLine;

namespace nresx.CommandLine.Commands
{
    public class BaseCommand
    {
        [Option( 's', "source", HelpText = "Source resource file" )]
        public string Source { get; set; }

        [Option( 'd', "destination", HelpText = "Destination resource file" )]
        public string Destination { get; set; }

        [Option( 'f', "format", HelpText = "New resource format" )]
        public string Format { get; set; }
    }
}