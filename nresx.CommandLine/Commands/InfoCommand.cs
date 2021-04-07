using System;
using CommandLine;

namespace nresx.CommandLine.Commands
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