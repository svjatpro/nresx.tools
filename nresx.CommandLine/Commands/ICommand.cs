using System;

namespace nresx.CommandLine.Commands
{
    public interface ICommand
    {
        void Execute();

        public void SetContext(CommandLineContext context) { }

        bool Successful { get; }
        Exception Exception { get; }
    }
}