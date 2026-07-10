using System;
using System.Collections.Generic;
using System.Linq;
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

            ForEachResourceGroup( sourceFiles, ( context, group ) =>
            {
                // load raw elements (duplicates preserved) per file
                var resources = group.Files
                    .Select( f => new GroupMember( f, ResourceFile.LoadRawElements( f.AbsolutePath ).ToList() ) )
                    .ToList();

                // union-of-keys map: key → (file → value); used for MissedElement
                var resourceMap = new Dictionary<string, Dictionary<ResourceFile, string>>();
                foreach ( var r in resources )
                {
                    foreach ( var element in r.Elements )
                    {
                        var key = element.Key ?? string.Empty;
                        resourceMap.TryAdd( key, new Dictionary<ResourceFile, string>() );
                        resourceMap[key].TryAdd( r.File, element.Value );
                    }
                }

                // base file for the group (the source language file), picked by ResourceGroup.Detect
                var baseFile = group.BaseFile;

                resources.ForEach( r =>
                {
                    var result = r.Elements.ValidateElements( out var errors );

                    // missed elements: any key present somewhere else in the group but not here
                    var missed = resourceMap.Keys.Except( r.Elements.Select( el => el.Key ) ).ToList();
                    if ( missed.Any() )
                    {
                        result = false;
                        errors.AddRange( missed.Select( el => new ResourceElementError( ResourceElementErrorType.MissedElement, el ) ) );
                    }

                    // not translated: only for non-base files, only against the base's value
                    if ( baseFile != null && !ReferenceEquals( r.File, baseFile ) )
                    {
                        foreach ( var el in r.Elements )
                        {
                            if ( !resourceMap.TryGetValue( el.Key, out var elMap ) ) continue;
                            if ( elMap.TryGetValue( baseFile, out var baseValue ) && baseValue == el.Value )
                            {
                                result = false;
                                errors.Add( new ResourceElementError( ResourceElementErrorType.NotTranslated, el.Key ) );
                            }
                        }
                    }

                    if ( !result )
                    {
                        foreach ( var elementError in errors )
                        {
                            var severity = elementError.ErrorType.GetSeverity();
                            var severityLabel = severity == ResourceElementErrorSeverity.Error ? "error" : "warning";
                            var detail = !string.IsNullOrWhiteSpace( elementError.ElementKey )
                                ? elementError.ElementKey
                                : ( elementError.Message ?? string.Empty );

                            Console.WriteLine( $"{r.File.AbsolutePath}: {severityLabel}: {elementError.ErrorType}: {detail}" );

                            totalIssues++;
                            if ( severity == ResourceElementErrorSeverity.Error )
                                totalErrors++;
                            else
                                totalWarnings++;

                            if ( severity == ResourceElementErrorSeverity.Error || WarningsAsErrors )
                                anyFailure = true;
                        }
                    }
                } );
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

        private sealed class GroupMember
        {
            public ResourceFile File { get; }
            public List<ResourceElement> Elements { get; }

            public GroupMember( ResourceFile file, List<ResourceElement> elements )
            {
                File = file;
                Elements = elements;
            }
        }
    }
}
