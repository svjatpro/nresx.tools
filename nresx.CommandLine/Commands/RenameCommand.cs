using System;
using System.Linq;
using CommandLine;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "rename", HelpText = "Rename an element in the resource file(s)" )]
    public class RenameCommand : BaseCommand, ICommand
    {
        [Option( 'k', "key", HelpText = "element key", Required = true )]
        public string Key { get; set; }

        [Option( 'n', "new-key", HelpText = "element key", Required = true )]
        public string NewKey { get; set; }

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
                        Console.WriteLine( $"fatal: '{Key}' element not found" );
                        return;
                    }
                    else
                    {
                        element.Key = NewKey;
                        Console.WriteLine( $"'{Key}' element have been renamed to '{NewKey}' in '{shortFilePath}'" );
                    }

                    if ( !DryRun )
                    {
                        resource.Save( file.FullName );
                    }
                } );
        }
    }
}