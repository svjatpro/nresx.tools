using System.IO;
using CommandLine;
using nresx.Tools;
using nresx.Tools.Helpers;

namespace nresx.CommandLine.Commands
{
    [Verb( "add", HelpText = "Add new element to the resource file" )]
    public class AddCommand : BaseCommand, ICommand
    {
        [Value( 0, HelpText = "Source resource file" )]
        public string SourceFileValue { get; set; }


        [Option( 'k', "key", HelpText = "element key", Required = true )]
        public string Key { get; set; }

        [Option( 'v', "value", HelpText = "element value", Required = true )]
        public string Value { get; set; }

        [Option( 'c', "comment", HelpText = "element comment" )]
        public string Comment { get; set; }


        [Option( "new", HelpText = "Create new resource file, if it is not exits yet" )]
        public bool New { get; set; }
        
        public override void Execute()
        {
            var source = SourceFileValue;
            if ( TryOpenResourceFile( source, out var res, createNonExisting: New ) )
            {
                res.Elements.Add( Key, Value, Comment );
                res.Save( source );
            }
            else
            {
                Successful = false;
                //
            }
        }
    }
}