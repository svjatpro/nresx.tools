using System;

namespace nresx.CommandLine.Commands
{
    public interface ICommand
    {
        void Execute();

        bool Successful { get; }
        Exception Exception { get; }
    }
}