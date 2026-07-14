using System.Collections.Generic;

namespace nresx.CommandLine.Analysis
{
    // Result of the zero-config project scan run by a bare `nresx validate` (RSX-250, RSX-264).
    // Populated by ProjectAnalyzer, rendered by ValidateCommand. Localization state and resource
    // presence are two independent facts (RSX-264): a project can be not-localized yet still have
    // resource files sitting in test/fixture folders.
    public class ProjectAnalysis
    {
        public string Root { get; set; }

        // The project itself is localized: it owns resource files outside test/fixture folders.
        public bool Localized { get; set; }
        // Neither project resources, fixture resources, nor recognized source were found.
        public bool NoProject { get; set; }

        // --- localized project (project-owned resource files) ---
        public int ResourceFileCount { get; set; }
        public int GroupCount { get; set; }
        public List<string> Formats { get; } = new();
        public List<string> Languages { get; } = new();
        public string Layout { get; set; }
        // short paths of the project resource files, listed when the count is small
        public List<string> ResourceFilePaths { get; } = new();
        // conservative C# consumption scan found resource references in source (confidence only)
        public bool SourceReferencesResources { get; set; }

        public int TotalIssues { get; set; }
        public int Errors { get; set; }
        public int Warnings { get; set; }
        public Dictionary<string, int> IssuesByRule { get; } = new();
        // formatted per-issue lines, only kept while the count is under the listing limit
        public List<string> IssueLines { get; } = new();

        // resource-scan budget outcome (RSX-264): how many of the found files were actually
        // validated, and whether the scan stopped early (partial). Partial => the report tells
        // the user the check was not exhaustive and to re-run with --verbose.
        public int ResourceFilesScanned { get; set; }
        public bool ResourceScanPartial { get; set; }

        // files found by extension but that could not be loaded / parsed (reported, not silently skipped)
        public List<string> UnreadableFiles { get; } = new();

        // --- resource files that live only under test/fixture folders (not the project's own) ---
        public int FixtureResourceFileCount { get; set; }
        public List<string> FixtureFormats { get; } = new();

        // --- not-localized project: the source-code scan ---
        public int SourceFileCount { get; set; }
        public long SourceBytes { get; set; }
        // how many source files were actually scanned, and whether the scan stopped early on the
        // budget (RSX-264). Partial => tokens found "so far", the report says the scan was partial.
        public int SourceFilesScanned { get; set; }
        public bool SourceScanPartial { get; set; }
        public int PotentialTokens { get; set; }
    }
}
