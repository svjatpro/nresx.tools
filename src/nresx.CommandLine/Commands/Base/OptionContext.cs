using System.Collections.Generic;

namespace nresx.CommandLine.Commands
{
    public class OptionContext
    {
        public readonly List<string> FreeArgs;
        public readonly bool Success;
        public readonly List<string> Errors;
        
        public OptionContext( List<string> freeArgs, bool hasErrors, List<string> errors = null )
        {
            FreeArgs = freeArgs;
            Success = hasErrors;
            Errors = errors ?? new List<string>();
        }
    }
}