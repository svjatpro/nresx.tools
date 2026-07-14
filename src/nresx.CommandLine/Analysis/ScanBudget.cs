using System;

namespace nresx.CommandLine.Analysis
{
    // Caps for the zero-config `nresx validate` scan (RSX-264). Plain validate is aimed at a user
    // who does not yet know the tool, so it must never hang for minutes on a large repo: both the
    // resource-file validation and the source-code scan stop once any cap is hit and the report
    // says the result is partial ("run with --verbose for the full scan"). `--verbose` uses
    // Unlimited so the full, unbounded scan runs on request.
    public class ScanBudget
    {
        public int MaxResourceFiles { get; set; } = 1000;
        public int MaxSourceFiles { get; set; } = 400;
        public long MaxSourceBytes { get; set; } = 8L * 1024 * 1024;
        public TimeSpan MaxDuration { get; set; } = TimeSpan.FromSeconds( 5 );

        public static ScanBudget Unlimited => new()
        {
            MaxResourceFiles = int.MaxValue,
            MaxSourceFiles = int.MaxValue,
            MaxSourceBytes = long.MaxValue,
            MaxDuration = TimeSpan.MaxValue
        };
    }
}
