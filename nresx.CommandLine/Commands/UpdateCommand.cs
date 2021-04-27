using System.Linq;
using CommandLine;
using nresx.Tools;

namespace nresx.CommandLine.Commands
{
    [Verb( "update", HelpText = "Update an element(s) in the resource file" )]
    public class UpdateCommand : ICommand
    {
        [Value( 0, HelpText = "Source resource file" )]
        public string SourceFileValue { get; set; }

        [Option( 'k', "key", HelpText = "element key", Required = true )]
        public string Key { get; set; }

        [Option( 'v', "value", HelpText = "element value" )]
        public string Value { get; set; }

        [Option( 'c', "comment", HelpText = "element comment" )]
        public string Comment { get; set; }

        public void Execute()
        {
            var source = SourceFileValue;
            if ( !string.IsNullOrWhiteSpace( source ) && (Key != null || Comment != null) )
            {
                var res = new ResourceFile( source );
                var element = res.Elements.FirstOrDefault( el => el.Key == Key );
                if ( element != null )
                {
                    if ( Value != null ) element.Value = Value;
                    if ( Comment != null ) element.Comment = Comment;
                    res.Save( source );
                }
            }
        }
    }
}