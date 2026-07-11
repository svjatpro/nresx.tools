using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Core.Extensions;

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

        protected override IEnumerable<string> HelpExamples =>
        [
            "# will update single element with new value in the \"file1\" resource file\n" +
            "nresx update file1 -k key1 -v value1",
            "# will update single element with new value and comment resource file\n" +
            "nresx update file1 -k key1 -c \"the comment1\" -v \"value1\"",
            "# will update single element in all resource files, which match the pathspec,\n" +
            "#  beginning from current directory, including all subdirectories\n" +
            "nresx update *.resw -r -k key1 -v value1",
        ];

        protected override void ExecuteCommand()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true, optionName: "source" )
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
                        if ( CreateNewElement )
                        {
                            resource.Elements.Add( Key, Value, Comment );
                            Console.WriteLine( $"'{Key}: {Value}' element have been added in '{shortFilePath}'" );
                        }
                        else
                        {
                            WriteError( ExitNotFound, ElementNotFoundErrorMessage, Key, shortFilePath );
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