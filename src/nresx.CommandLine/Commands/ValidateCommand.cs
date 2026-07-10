using System;
using CommandLine;
using nresx.CommandLine.Commands.Base;
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

        protected override bool IsRecursiveAllowed => true;

        protected override void ExecuteCommand()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true )
                .Validate( this );
            if ( !optionsParsed )
                return;

            var anyFailure = false;
            var totalIssues = 0;
            var totalErrors = 0;
            var totalWarnings = 0;

            // Cross-file validation lives in nresx.Core (RSX-235); the command only renders the
            // findings, counts severities, and maps them to the exit code.
            ForEachResourceGroup( sourceFiles, ( context, group ) =>
            {
                foreach ( var issue in group.Validate() )
                {
                    var error = issue.Error;
                    var severity = error.ErrorType.GetSeverity();
                    var severityLabel = severity == ResourceElementErrorSeverity.Error ? "error" : "warning";
                    var detail = !string.IsNullOrWhiteSpace( error.ElementKey )
                        ? error.ElementKey
                        : ( error.Message ?? string.Empty );

                    Console.WriteLine( $"{issue.FilePath}: {severityLabel}: {error.ErrorType}: {detail}" );

                    totalIssues++;
                    if ( severity == ResourceElementErrorSeverity.Error )
                        totalErrors++;
                    else
                        totalWarnings++;

                    if ( severity == ResourceElementErrorSeverity.Error || WarningsAsErrors )
                        anyFailure = true;
                }
            }, BasicLanguage );

            if ( totalIssues > 0 )
            {
                Console.WriteLine( BuildSummary( totalIssues, totalErrors, totalWarnings ) );
            }

            if ( anyFailure )
                Successful = false;
        }

        private static string BuildSummary( int issues, int errors, int warnings )
        {
            string Plural( int n, string word ) => n == 1 ? word : word + "s";
            return $"Found {issues} {Plural( issues, "issue" )} ({errors} {Plural( errors, "error" )}, {warnings} {Plural( warnings, "warning" )})";
        }
    }
}
