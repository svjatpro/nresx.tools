using System;
using System.Collections.Generic;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Core.Extensions;

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

        protected override bool IsCreateNewElementAllowed => true;
        protected override bool IsCreateNewFileAllowed => true;
        protected override bool IsFormatAllowed => true;
        protected override bool IsRecursiveAllowed => true;

        protected override IEnumerable<string> HelpExamples =>
        [
            "# will insert single element with \"key1\" key and \"value1\" value to the \"file1\" resource file\n" +
            "nresx add file1 -k key1 -v value1",
            "# will insert single element with a comment\n" +
            "nresx add file1 -k key1 -v value1 -c \"the comment1\"",
            "# will insert single element to all resource files, which match the pathspec,\n" +
            "#  beginning from current directory, including all subdirectories\n" +
            "nresx add *.resw -r -k key1 -v value1",
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
                    if ( !DryRun )
                    {
                        resource.Elements.Add( Key, Value, Comment );
                        resource.Save( file.FullName );
                    }
                    var shortFilePath = resource.AbsolutePath?.GetShortPath() ?? file.FullName.GetShortPath();
                    Console.WriteLine( $"'{Key}: {Value}' element have been add to '{shortFilePath}'" );
                } );
        }
    }
}