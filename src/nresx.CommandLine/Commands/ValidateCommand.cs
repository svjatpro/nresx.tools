using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Core;
using nresx.Core.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "validate", HelpText = "Validate resource file(s)" )]
    public class ValidateCommand : BaseCommand
    {
        [Option( "basic-lan", HelpText = "Base language code (e.g. en, en-US). Overrides auto-detection of the source-language file in a translation group." )]
        public string BasicLanguage { get; set; }

        [Option( "warnings-as-errors", HelpText = "Treat warnings as errors when setting the exit code." )]
        public bool WarningsAsErrors { get; set; }

        // Shadows the inherited resource-format option so `validate --help` describes the
        // flag's meaning HERE (output format). Same short/long names - the parser binds this
        // one on the concrete type (RSX-232, plan step 5: drop if the parser rejects it).
        [Option( 'f', "format", HelpText = "Output format: text (default) or json" )]
        public new string Format { get => base.Format; set => base.Format = value; }

        protected override bool IsRecursiveAllowed => true;

        protected override void ExecuteCommand()
        {
            // Source is optional: a bare `nresx validate` (no source, no options) runs the
            // zero-config project analyzer over the current directory instead of erroring (RSX-250).
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: false, multipleIndirect: true, optionName: "source" )
                .Validate( this );
            if ( !optionsParsed )
                return;

            // -f/--format selects the OUTPUT format here (validate never converts resources,
            // so the inherited resource-format meaning of the flag is unused; IsFormatAllowed
            // stays false to keep it out of the ForEachSourceFile code path).
            var outputJson = false;
            if ( !string.IsNullOrWhiteSpace( Format ) )
            {
                switch ( Format.ToLowerInvariant() )
                {
                    case "text": break;
                    case "json": outputJson = true; break;
                    default:
                        WriteError( ExitUsageError, UnknownOutputFormatErrorMessage, Format );
                        return;
                }
            }

            if ( sourceFiles.Count == 0 )
            {
                RunProjectAnalysis();
                return;
            }

            var anyFailure = false;
            var issues = new List<( string file, string severityLabel, ResourceElementError error )>();
            var totalErrors = 0;
            var totalWarnings = 0;
            var totalFiles = 0;

            // Cross-file validation lives in nresx.Core (RSX-235); the command collects the
            // findings, then renders them in the selected output format (RSX-232). Progress ticks
            // per file as it loads (the expensive phase) so the run never looks hung, and one
            // malformed file is reported and skipped instead of crashing the whole run (RSX-264).
            var progress = new ScanProgress( enabled: !outputJson );
            var unreadable = new List<string>();
            ForEachResourceGroup( sourceFiles, ( context, group ) =>
            {
                totalFiles += group.Files.Count;
                if ( Verbose )
                    foreach ( var file in group.Files )
                        WriteVerbose( "validated: {0}", file.AbsolutePath );

                foreach ( var issue in group.Validate() )
                {
                    var error = issue.Error;
                    var severity = error.ErrorType.GetSeverity();
                    var severityLabel = severity == ResourceElementErrorSeverity.Error ? "error" : "warning";

                    issues.Add( ( issue.FilePath, severityLabel, error ) );

                    if ( severity == ResourceElementErrorSeverity.Error )
                        totalErrors++;
                    else
                        totalWarnings++;

                    if ( severity == ResourceElementErrorSeverity.Error || WarningsAsErrors )
                        anyFailure = true;
                }
            }, BasicLanguage,
               onFileLoading: _ => progress.Tick(),
               onLoadError: ( path, ex ) =>
               {
                   unreadable.Add( $"{path.GetShortPath()} ({ex.GetType().Name})" );
                   return true;
               } );
            progress.Clear();

            // Surface unreadable files instead of silently dropping them (RSX-245 B3); a file we
            // could not parse is a real problem, so the run fails but still reports the rest.
            foreach ( var file in unreadable )
                Console.Error.WriteLine( $"error: could not read {file}" );
            if ( unreadable.Count > 0 )
                anyFailure = true;

            if ( outputJson )
                RenderJson( issues, totalErrors, totalWarnings );
            else
                // suppress the clean-run confirmation when a search error was already
                // reported (Successful false) - "0 issues" after a fatal line is noise
                RenderText( issues, totalErrors, totalWarnings, totalFiles, printCleanSummary: Successful, verbose: Verbose );

            if ( anyFailure )
                Successful = false;
        }

        // Zero-config path: analyze the current directory and print a short, truthful report
        // for both already-localized and not-yet-localized projects (RSX-250). Any real
        // validation errors still fail the command so a bare `validate` works as a CI gate too.
        private void RunProjectAnalysis()
        {
            var analysis = new Analysis.ProjectAnalyzer().Analyze( Environment.CurrentDirectory );
            RenderProjectAnalysis( analysis );

            if ( analysis.Errors > 0 || ( analysis.Warnings > 0 && WarningsAsErrors ) )
                Successful = false;
        }

        private static void RenderProjectAnalysis( Analysis.ProjectAnalysis a )
        {
            if ( a.Localized )
                RenderLocalized( a );
            else
                RenderNotLocalized( a );
        }

        private static void RenderLocalized( Analysis.ProjectAnalysis a )
        {
            Console.WriteLine( $"Localized project: {a.ResourceFileCount} resource {Plural( a.ResourceFileCount, "file" )} " +
                $"in {a.GroupCount} {Plural( a.GroupCount, "group" )}." );
            Console.WriteLine( $"  formats:   {string.Join( ", ", a.Formats )}" );
            Console.WriteLine( a.Languages.Count > 0
                ? $"  languages: {string.Join( ", ", a.Languages )}"
                : "  languages: none detected (single language / no culture in paths)" );
            Console.WriteLine( $"  layout:    {a.Layout}" );
            if ( a.SourceReferencesResources )
                Console.WriteLine( "  reference: resource usage found in source" );

            // Show the resource files themselves when there are only a few (RSX-264).
            if ( a.ResourceFilePaths.Count > 0 )
            {
                Console.WriteLine( $"  files:" );
                foreach ( var path in a.ResourceFilePaths )
                    Console.WriteLine( $"    {path}" );
            }

            if ( a.UnreadableFiles.Count > 0 )
            {
                Console.WriteLine( $"  skipped {a.UnreadableFiles.Count} unreadable {Plural( a.UnreadableFiles.Count, "file" )}:" );
                foreach ( var file in a.UnreadableFiles.Take( ProjectAnalyzerListLimit ) )
                    Console.WriteLine( $"    {file.GetShortPath()}" );
                if ( a.UnreadableFiles.Count > ProjectAnalyzerListLimit )
                    Console.WriteLine( $"    ... and {a.UnreadableFiles.Count - ProjectAnalyzerListLimit} more" );
            }

            Console.WriteLine();
            if ( a.TotalIssues == 0 )
            {
                if ( !a.ResourceScanPartial )
                {
                    Console.WriteLine( "Validation: no issues found." );
                    return;
                }
                // Bounded scan (RSX-264): absence of issues here is not authoritative.
                Console.WriteLine( $"No issues in the first {a.ResourceFilesScanned} of {a.ResourceFileCount} " +
                    $"resource {Plural( a.ResourceFileCount, "file" )} - partial scan." );
                Console.WriteLine( $"For a full check, run `nresx validate \"{GlobHint( a )}\" -r`." );
                return;
            }

            if ( a.TotalIssues <= Analysis.ProjectAnalyzer.IssueListLimit )
            {
                foreach ( var line in a.IssueLines )
                    Console.WriteLine( line );
            }
            else
            {
                // Collapse to totals + per-rule breakdown so a big or fixture-heavy tree stays short.
                foreach ( var rule in a.IssuesByRule.OrderByDescending( kv => kv.Value ) )
                    Console.WriteLine( $"  {rule.Key}: {rule.Value} " + Plural( rule.Value, "issue" ) );
            }
            Console.WriteLine( a.ResourceScanPartial
                ? $"Found at least {a.TotalIssues} {Plural( a.TotalIssues, "issue" )} " +
                    $"({a.Errors} {Plural( a.Errors, "error" )}, {a.Warnings} {Plural( a.Warnings, "warning" )}) " +
                    $"in the first {a.ResourceFilesScanned} of {a.ResourceFileCount} resource {Plural( a.ResourceFileCount, "file" )}."
                : $"Found {a.TotalIssues} {Plural( a.TotalIssues, "issue" )} " +
                    $"({a.Errors} {Plural( a.Errors, "error" )}, {a.Warnings} {Plural( a.Warnings, "warning" )})." );
            Console.WriteLine( $"For the full report, run `nresx validate \"{GlobHint( a )}\" -r`." );
        }

        private static void RenderNotLocalized( Analysis.ProjectAnalysis a )
        {
            if ( a.NoProject )
            {
                Console.WriteLine( "No project found here: no resource files, and no recognized source files (.cs, .xaml)." );
                return;
            }

            // Localization state and resource presence are two separate facts (RSX-264).
            Console.WriteLine( "Not localized." );
            if ( a.FixtureResourceFileCount > 0 )
                Console.WriteLine( $"Found {a.FixtureResourceFileCount} localization {Plural( a.FixtureResourceFileCount, "file" )} " +
                    $"under test/fixtures (formats: {string.Join( ", ", a.FixtureFormats )}), but the project itself is not localized." );
            else
                Console.WriteLine( "No resource files found." );

            if ( a.SourceFileCount == 0 )
                return;

            if ( a.SourceScanPartial )
            {
                // Budgeted scan (RSX-264): report what was found so far without hanging on a big repo.
                Console.WriteLine( $"Scanned the first {a.SourceFilesScanned} of {a.SourceFileCount} source " +
                    $"{Plural( a.SourceFileCount, "file" )} (partial); ~{a.PotentialTokens} potential " +
                    $"{Plural( a.PotentialTokens, "token" )} found so far." );
                Console.WriteLine( "To localize, run `nresx generate * <file.resx> -r` to extract them (narrow the path for a full scan)." );
                return;
            }

            Console.WriteLine( $"Scanned {a.SourceFileCount} source {Plural( a.SourceFileCount, "file" )}; " +
                $"~{a.PotentialTokens} potential {Plural( a.PotentialTokens, "token" )} to localize." );
            Console.WriteLine( "To localize, run `nresx generate * <file.resx> -r` to extract them into a resource file." );
        }

        // A representative recursive glob for the "full report" hint, using the dominant format.
        private static string GlobHint( Analysis.ProjectAnalysis a )
        {
            var ext = a.Formats.FirstOrDefault() ?? ".resx";
            return $"**/*{ext}";
        }

        private static string Plural( int n, string word ) => n == 1 ? word : word + "s";

        private const int ProjectAnalyzerListLimit = 10;

        private static void RenderText( List<( string file, string severityLabel, ResourceElementError error )> issues, int errors, int warnings, int files, bool printCleanSummary, bool verbose )
        {
            if ( issues.Count == 0 )
            {
                if ( printCleanSummary )
                    // A clean run must still confirm that something was actually checked -
                    // silence is indistinguishable from "nothing matched" (RSX-245).
                    Console.WriteLine( $"Found 0 issues ({files} {Plural( files, "file" )} checked)" );
                return;
            }

            // List every finding when verbose or when there are only a few; otherwise collapse to a
            // per-rule breakdown so a big run stays readable, and point at --verbose (RSX-264).
            if ( verbose || issues.Count <= Analysis.ProjectAnalyzer.IssueListLimit )
            {
                foreach ( var ( file, severityLabel, error ) in issues )
                {
                    var detail = !string.IsNullOrWhiteSpace( error.ElementKey )
                        ? error.ElementKey
                        : ( error.Message ?? string.Empty );
                    Console.WriteLine( $"{file}: {severityLabel}: {error.ErrorType}: {detail}" );
                }
                Console.WriteLine( BuildSummary( issues.Count, errors, warnings ) );
                return;
            }

            foreach ( var rule in issues.GroupBy( i => i.error.ErrorType )
                         .OrderByDescending( g => g.Count() ) )
                Console.WriteLine( $"  {rule.Key}: {rule.Count()} " + Plural( rule.Count(), "issue" ) );
            Console.WriteLine( BuildSummary( issues.Count, errors, warnings ) );
            Console.WriteLine( "To list each result, add --verbose." );
        }

        // Live single-line progress on stderr, only when stderr is a real terminal (RSX-264). Kept
        // off when output is redirected/piped (tests, JSON, CI) so it never pollutes captured output.
        private sealed class ScanProgress
        {
            private static readonly char[] Frames = { '|', '/', '-', '\\' };
            private readonly bool _active;
            private int _count;

            public ScanProgress( bool enabled )
            {
                _active = enabled && !Console.IsErrorRedirected;
            }

            public void Tick()
            {
                if ( !_active ) return;
                _count++;
                Console.Error.Write( $"\r{Frames[_count % Frames.Length]} scanning: {_count} files..." );
            }

            public void Clear()
            {
                if ( !_active ) return;
                Console.Error.Write( "\r" + new string( ' ', 32 ) + "\r" );
            }
        }

        // Schema v1 (RSX-232); guard extends it with sarif and more rules later (RSX-158).
        private static void RenderJson( List<( string file, string severityLabel, ResourceElementError error )> issues, int errors, int warnings )
        {
            var document = new
            {
                tool = "nresx",
                version = ResourceManager.GetVersion().TrimStart( 'v' ),
                issues = issues.Select( i => new
                {
                    file = i.file,
                    severity = i.severityLabel,
                    rule = i.error.ErrorType.ToString(),
                    key = string.IsNullOrWhiteSpace( i.error.ElementKey ) ? null : i.error.ElementKey,
                    message = string.IsNullOrWhiteSpace( i.error.Message ) ? null : i.error.Message
                } ),
                summary = new { issues = issues.Count, errors, warnings }
            };

            Console.WriteLine( JsonSerializer.Serialize( document, new JsonSerializerOptions { WriteIndented = true } ) );
        }

        private static string BuildSummary( int issues, int errors, int warnings )
        {
            string Plural( int n, string word ) => n == 1 ? word : word + "s";
            return $"Found {issues} {Plural( issues, "issue" )} ({errors} {Plural( errors, "error" )}, {warnings} {Plural( warnings, "warning" )})";
        }

        protected override IEnumerable<string> HelpExamples =>
        [
            "# zero-config: analyze the current directory - reports the localization layout,\n" +
            "#  languages and validation result for a localized project, or the potential tokens\n" +
            "#  to extract for a not-yet-localized one\n" +
            "nresx validate",
            "# will validate elements within a single resource file: empty or duplicated elements\n" +
            "nresx validate <file1>",
            "# will validate all matched resource files, including cross-file checks within\n" +
            "#  each translation group (missed elements, not translated elements)\n" +
            "nresx validate dir1\\*.resw -r",
            "# CI gate: any finding fails the build, and 'uk' is the source language\n" +
            "nresx validate *.resx -r --basic-lan uk --warnings-as-errors",
            "# machine-readable output for CI tooling / bots\n" +
            "nresx validate *.resx -r --format json",
        ];
    }
}
