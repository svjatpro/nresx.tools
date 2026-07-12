using System.Collections.Generic;

namespace nresx.CommandLine.Analysis
{
    // Result of the zero-config project scan run by a bare `nresx validate` (RSX-250).
    // Populated by ProjectAnalyzer, rendered by ValidateCommand. Two shapes share one type:
    // when Localized is true the resource-file fields are filled; otherwise the source-scan
    // fields describe a not-yet-localized project.
    public class ProjectAnalysis
    {
        public string Root { get; set; }
        public bool Localized { get; set; }

        // --- localized project ---
        public int ResourceFileCount { get; set; }
        public int GroupCount { get; set; }
        public List<string> Formats { get; } = new();
        public List<string> Languages { get; } = new();
        public string Layout { get; set; }

        public int TotalIssues { get; set; }
        public int Errors { get; set; }
        public int Warnings { get; set; }
        public Dictionary<string, int> IssuesByRule { get; } = new();
        // formatted per-issue lines, only kept while the count is under the listing limit
        public List<string> IssueLines { get; } = new();

        // files found by extension but that could not be loaded / parsed (reported, not silently skipped)
        public List<string> UnreadableFiles { get; } = new();

        // --- not-localized project ---
        public int SourceFileCount { get; set; }
        public long SourceBytes { get; set; }
        // true when the codebase is too large to extract in-line; the scan reports counts only
        public bool SourceTooLarge { get; set; }
        public int PotentialTokens { get; set; }
    }
}
