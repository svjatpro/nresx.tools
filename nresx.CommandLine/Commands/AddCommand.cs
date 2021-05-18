using CommandLine;

namespace nresx.CommandLine.Commands
{
    [Verb( "add", HelpText = "Add new element to the resource file" )]
    public class AddCommand : BaseCommand, ICommand
    {
        [Option( 'k', "key", HelpText = "element key", Required = true )]
        public string Key { get; set; }

        [Option( 'v', "value", HelpText = "element value", Required = true )]
        public string Value { get; set; }

        [Option( 'c', "comment", HelpText = "element comment" )]
        public string Comment { get; set; }


        public override void Execute()
        {
            var sourceFiles = GetSourceFiles();
            ForEachSourceFile(
                sourceFiles,
                ( file, resource ) =>
                {
                    resource.Elements.Add( Key, Value, Comment );
                    resource.Save( file.FullName );
                } );
        }
    }
}