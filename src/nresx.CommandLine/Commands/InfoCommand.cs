using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Core.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "info", HelpText = "get resource info" )]
    public class InfoCommand : BaseCommand, ICommand
    {
        protected override bool IsRecursiveAllowed => true;

        protected override IEnumerable<string> HelpExamples =>
        [
            "# Will put information about two files to the stdout\n" +
            "nresx <file1> <file2>",
            "# Will put to the stdout information about all *.yaml files in the current directory, including all subdirectories\n" +
            "nresx info *.resx -r",
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
                ( ctx, resource ) =>
                {
                    if ( ctx.FilesProcessed + ctx.FilesFailed > 0 )
                        Console.WriteLine( new string( '-', 30 ) );
                    Console.WriteLine( $"Resource file name: \"{resource.FileName}\", (\"{resource.AbsolutePath})\"" );
                    Console.WriteLine( $"resource format type: {resource.FileFormat}" );
                    
                    if ( resource.AbsolutePath.TryToExtractCultureFromPath( out var culture ) )
                        Console.WriteLine( $"resource culture: {culture.DisplayName}" );

                    Console.WriteLine( $"text elements: {resource.Elements.Count()}" );
                },
                splitFiles: true);
        }
    }
}