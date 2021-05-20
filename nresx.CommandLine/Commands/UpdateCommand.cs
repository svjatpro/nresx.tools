using System;
using System.Linq;
using CommandLine;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "update", HelpText = "Update an element(s) in the resource file" )]
    public class UpdateCommand : BaseCommand, ICommand
    {
        [Option( 'k', "key", HelpText = "element key", Required = true )]
        public string Key { get; set; }

        [Option( 'v', "value", HelpText = "element value", Group = "value" )]
        public string Value { get; set; }

        [Option( 'c', "comment", HelpText = "element comment", Group = "value" )]
        public string Comment { get; set; }

        protected override bool IsCreateNewElementAllowed => true;
        protected override bool IsRecursiveAllowed => true;

        public override void Execute()
        {
            var sourceFiles = GetSourceFiles();
            ForEachSourceFile(
                sourceFiles,
                ( file, resource ) =>
                {
                    var shortFilePath = resource.AbsolutePath?.GetShortPath() ?? file.FullName.GetShortPath();
                    var element = resource.Elements.FirstOrDefault( el => el.Key == Key );
                    if ( element == null )
                    {
                        if ( CreateNewElement )
                        {
                            resource.Elements.Add( Key, Value, Comment );
                            Console.WriteLine( $"'{Key}: {Value}' element have been added in '{shortFilePath}'" );
                        }
                        else
                        {
                            Console.WriteLine( $"fatal: '{Key}' element not found" );
                            return;
                        }
                    }
                    else
                    {
                        if ( Value != null ) element.Value = Value;
                        if ( Comment != null ) element.Comment = Comment;

                        Console.WriteLine( $"'{Key}' element have been updated in '{shortFilePath}'" );
                    }

                    if ( !DryRun )
                    {
                        resource.Save( file.FullName );
                    }
                } );
        }
    }
}