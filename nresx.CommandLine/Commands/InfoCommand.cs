using System;
using CommandLine;

namespace NResx.Tools.CommandLine.Commands
{
    [Verb( "info", HelpText = "get resource info" )]
    public class InfoCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine( "executing info" );
        }
    }
}