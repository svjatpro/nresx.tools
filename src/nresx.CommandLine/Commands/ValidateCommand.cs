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
            // findings, then renders them in the selected output format (RSX-232).
            ForEachResourceGroup( sourceFiles, ( context, group ) =>
            {
                totalFiles += group.Files.Count;
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
            }, BasicLanguage );

            if ( outputJson )
                RenderJson( issues, totalErrors, totalWarnings );
            else
                // suppress the clean-run confirmation when a search error was already
                // reported (Successful false) - "0 issues" after a fatal line is noise
                RenderText( issues, totalErrors, totalWarnings, totalFiles, printCleanSummary: Successful );

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
            string Plural( int n, string word ) => n == 1 ? word : word + "s";

            if ( !a.Localized )
            {
                Console.WriteLine( "Not localized: no resource files found." );
                if ( a.SourceFileCount == 0 )
                {
                    Console.WriteLine( "No parseable source files (.cs, .xaml) found either." );
                    return;
                }

                if ( a.SourceTooLarge )
                {
                    Console.WriteLine( $"Scanned {a.SourceFileCount} source {Plural( a.SourceFileCount, "file" )} " +
                        $"(~{a.SourceBytes / 1024} KB) - too large to extract in-line." );
                    Console.WriteLine( "Run `nresx generate * <file.resx> -r --dry-run` to preview extractable tokens." );
                    return;
                }

                Console.WriteLine( $"Scanned {a.SourceFileCount} source {Plural( a.SourceFileCount, "file" )}; " +
                    $"~{a.PotentialTokens} potential {Plural( a.PotentialTokens, "token" )} to localize." );
                Console.WriteLine( "Run `nresx generate * <file.resx> -r` to extract them into a resource file." );
                return;
            }

            Console.WriteLine( $"Localized project: {a.ResourceFileCount} resource {Plural( a.ResourceFileCount, "file" )} " +
                $"in {a.GroupCount} {Plural( a.GroupCount, "group" )}." );
            Console.WriteLine( $"  formats:   {string.Join( ", ", a.Formats )}" );
            Console.WriteLine( a.Languages.Count > 0
                ? $"  languages: {string.Join( ", ", a.Languages )}"
                : "  languages: none detected (single language / no culture in paths)" );
            Console.WriteLine( $"  layout:    {a.Layout}" );

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
                Console.WriteLine( "Validation: no issues found." );
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
            Console.WriteLine( $"Found {a.TotalIssues} {Plural( a.TotalIssues, "issue" )} " +
                $"({a.Errors} {Plural( a.Errors, "error" )}, {a.Warnings} {Plural( a.Warnings, "warning" )})." );
        }

        private const int ProjectAnalyzerListLimit = 10;

        private static void RenderText( List<( string file, string severityLabel, ResourceElementError error )> issues, int errors, int warnings, int files, bool printCleanSummary )
        {
            foreach ( var ( file, severityLabel, error ) in issues )
            {
                var detail = !string.IsNullOrWhiteSpace( error.ElementKey )
                    ? error.ElementKey
                    : ( error.Message ?? string.Empty );
                Console.WriteLine( $"{file}: {severityLabel}: {error.ErrorType}: {detail}" );
            }

            if ( issues.Count > 0 )
            {
                Console.WriteLine( BuildSummary( issues.Count, errors, warnings ) );
            }
            else if ( printCleanSummary )
            {
                // A clean run must still confirm that something was actually checked -
                // silence is indistinguishable from "nothing matched" (RSX-245).
                string Plural( int n, string word ) => n == 1 ? word : word + "s";
                Console.WriteLine( $"Found 0 issues ({files} {Plural( files, "file" )} checked)" );
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
