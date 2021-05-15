using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CommandLine;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "info", HelpText = "get resource info" )]
    public class InfoCommand : BaseCommand, ICommand
    {
        public override void Execute()
        {
            ForEachSourceFile(
                GetSourceFiles(),
                resource =>
                {
                    Console.WriteLine( $"Resource file name: \"{resource.Name}\", (\"{resource.AbsolutePath})\"" );
                    Console.WriteLine( $"resource format type: {resource.ResourceFormat}" );
                    
                    if ( resource.AbsolutePath.TryToGetCulture( out var culture ) )
                        Console.WriteLine( $"resource culture: {culture.DisplayName}" );

                    Console.WriteLine( $"text elements: {resource.Elements.Count()}" );

                    Console.WriteLine( new string( '-', 30 ) );
                } );
        }
    }
}