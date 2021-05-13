using System;
using System.Linq;
using CommandLine;

namespace nresx.CommandLine.Commands
{
    [Verb( "info", HelpText = "get resource info" )]
    public class InfoCommand : BaseCommand, ICommand
    {
        public void Execute()
        {
            ForEachSourceFile(
                GetSourceFiles(),
                resource =>
                {
                    Console.WriteLine( $"Resource file name: \"{resource.Name}\", (\"{resource.AbsolutePath})\"" );
                    Console.WriteLine( $"resource format type: {resource.ResourceFormat}" );
                    Console.WriteLine( $"text elements: {resource.Elements.Count()}" );
                    Console.WriteLine( new string( '-', 30 ) );
                } );
        }
    }
}