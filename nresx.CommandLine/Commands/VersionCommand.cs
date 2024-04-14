using System;
using CommandLine;
using nresx.Tools;

namespace nresx.CommandLine.Commands
{
    [Verb( "version", HelpText = "get version" )]
    public class VersionCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine( $@"nresx version: {ResourceManager.GetVersion()}" );
        }

        public bool Successful { get; protected set; } = true;
        public Exception Exception { get; protected set; } = null;
    }
}