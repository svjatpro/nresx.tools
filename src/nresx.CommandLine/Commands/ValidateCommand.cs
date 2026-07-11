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
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true )
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

            var anyFailure = false;
            var issues = new List<( string file, string severityLabel, ResourceElementError error )>();
            var totalErrors = 0;
            var totalWarnings = 0;

            // Cross-file validation lives in nresx.Core (RSX-235); the command collects the
            // findings, then renders them in the selected output format (RSX-232).
            ForEachResourceGroup( sourceFiles, ( context, group ) =>
            {
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
                RenderText( issues, totalErrors, totalWarnings );

            if ( anyFailure )
                Successful = false;
        }

        private static void RenderText( List<( string file, string severityLabel, ResourceElementError error )> issues, int errors, int warnings )
        {
            foreach ( var ( file, severityLabel, error ) in issues )
            {
                var detail = !string.IsNullOrWhiteSpace( error.ElementKey )
                    ? error.ElementKey
                    : ( error.Message ?? string.Empty );
                Console.WriteLine( $"{file}: {severityLabel}: {error.ErrorType}: {detail}" );
            }

            if ( issues.Count > 0 )
                Console.WriteLine( BuildSummary( issues.Count, errors, warnings ) );
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
