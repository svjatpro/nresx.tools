using System;
using System.Linq;
using CommandLine;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "info", HelpText = "get resource info" )]
    public class InfoCommand : BaseCommand, ICommand
    {
        protected override bool IsRecursiveAllowed => true;

        public override void Execute()
        {
            ForEachSourceFile(
                GetSourceFiles(),
                ( ctx, resource ) =>
                {
                    if ( ctx.FilesProcessed + ctx.FilesFaled > 0 )
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