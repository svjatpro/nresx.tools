using CommandLine;
using nresx.Tools;

namespace nresx.CommandLine.Commands
{
    [Verb( "add", HelpText = "Add new element to the resource file" )]
    public class AddCommand : ICommand
    {
        [Value( 0, HelpText = "Source resource file" )]
        public string SourceFileValue { get; set; }


        [Option( 'k', "key", HelpText = "element key", Required = true )]
        public string Key { get; set; }

        [Option( 'v', "value", HelpText = "element value", Required = true )]
        public string Value { get; set; }

        [Option( 'c', "comment", HelpText = "element comment" )]
        public string Comment { get; set; }

        public void Execute()
        {
            var source = SourceFileValue;
            if ( !string.IsNullOrWhiteSpace( source ) )
            {
                var res = new ResourceFile( source );
                res.AddElement( Key, Value, Comment );
                res.Save( source );
            }
        }
    }
}