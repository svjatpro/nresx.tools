using System;
using System.Linq;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "info", HelpText = "get resource info" )]
    public class InfoCommand : BaseCommand, ICommand
    {
        protected override bool IsRecursiveAllowed => true;

        public override void Execute()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true )
                .Validate();
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