using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Core.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "rename", HelpText = "Rename an element in the resource file(s)" )]
    public class RenameCommand : BaseCommand, ICommand
    {
        [Option( 'k', "key", HelpText = "element key", Required = true )]
        public string Key { get; set; }

        [Option( 'n', "new-key", HelpText = "new element key", Required = true )]
        public string NewKey { get; set; }

        protected override bool IsRecursiveAllowed => true;

        protected override IEnumerable<string> HelpExamples =>
        [
            "# will rename single element in the \"file1\" resource file\n" +
            "nresx rename file1 -k key1 -n key2",
            "# will rename single element in all *.resx files, starting from current directory\n" +
            "nresx rename *.resx -k key1 -n key2 --recursive",
        ];

        protected override void ExecuteCommand()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true )
                .Validate( this );
            if ( !optionsParsed )
                return;

            ForEachSourceFile(
                sourceFiles,
                ( file, resource ) =>
                {
                    var shortFilePath = resource.AbsolutePath?.GetShortPath() ?? file.FullName.GetShortPath();
                    var element = resource.Elements.FirstOrDefault( el => el.Key == Key );

                    if ( element == null )
                    {
                        WriteError( ExitNotFound, ElementNotFoundErrorMessage, Key, shortFilePath );
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