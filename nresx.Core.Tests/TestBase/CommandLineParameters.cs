using System.Collections.Generic;

namespace nresx.Core.Tests
{
    public class CommandLineParameters
    {
        public readonly List<string> UniqueKeys = new();
        public readonly List<string> SourceFiles = new();
        public readonly List<string> DestinationFiles = new();
        public readonly List<string> TemporaryFiles = new();
    }
}