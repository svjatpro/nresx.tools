using System.Collections.Generic;

namespace nresx.Core.Tests
{
    public class CommandLineParameters
    {
        public readonly List<string> UniqueKeys = new();
        public readonly List<string> RandomExtensions = new();
        public readonly List<string> SourceFiles = new();
        public readonly List<string> NewFiles = new();
        public readonly List<string> NewDirectories = new();
        public readonly List<string> TemporaryFiles = new();
        public readonly List<string> TemporaryProjects = new();

        public bool DryRun { get; set; }
        public bool Recursive { get; set; }

        public string CommandLine { get; set; }
        public readonly List<string> ConsoleOutput = new();
        public int ExitCode { get; set; }
    }
}