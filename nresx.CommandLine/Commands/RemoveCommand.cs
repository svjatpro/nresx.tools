using CommandLine;
using nresx.Tools;

namespace nresx.CommandLine.Commands
{
    [Verb( "remove", HelpText = "Remove an element from the resource file" )]
    public class RemoveCommand : ICommand
    {
        [Value( 0, HelpText = "Source resource file" )]
        public string SourceFileValue { get; set; }

        [Option( 'k', "key", HelpText = "element key", Required = true )]
        public string Key { get; set; }

        public void Execute()
        {
            var source = SourceFileValue;
            if ( !string.IsNullOrWhiteSpace( source ) )
            {
                var res = new ResourceFile( source );
                res.RemoveElement( Key );
                res.Save( source );
            }
        }
    }
}