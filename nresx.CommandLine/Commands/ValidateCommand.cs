using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
                .Validate();
            if ( !optionsParsed )
                return;

            var anyFailure = false;
            var totalIssues = 0;
            var totalErrors = 0;
            var totalWarnings = 0;

            ForEachResourceGroup( sourceFiles, ( context, group ) =>
            {
                // load elements + capture detected culture per file
                var resources = group
                    .Select( f =>
                    {
                        var elements = ResourceFile.LoadRawElements( f.FileInfo.FullName ).ToList();
                        f.FileInfo.FullName.TryToExtractCultureFromPath( out var culture );
                        return new GroupMember( f.FileInfo, elements, culture );
                    } )
                    .ToList();

                // union-of-keys map: key → (fileHash → value); used for MissedElement
                var resourceMap = new Dictionary<string, Dictionary<int, string>>();
                foreach ( var r in resources )
                {
                    foreach ( var element in r.Elements )
                    {
                        var key = element.Key ?? string.Empty;
                        resourceMap.TryAdd( key, new Dictionary<int, string>() );
                        resourceMap[key].TryAdd( r.FileInfo.GetHashCode(), element.Value );
                    }
                }

                // pick base file for the group (the source language file)
                var baseFile = PickBaseFile( resources, BasicLanguage );

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
                    if ( baseFile != null && r.FileInfo.GetHashCode() != baseFile.GetHashCode() )
                    {
                        foreach ( var el in r.Elements )
                        {
                            if ( !resourceMap.TryGetValue( el.Key, out var elMap ) ) continue;
                            if ( elMap.TryGetValue( baseFile.GetHashCode(), out var baseValue ) && baseValue == el.Value )
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

                            Console.WriteLine( $"{r.FileInfo.FullName}: {severityLabel}: {elementError.ErrorType}: {detail}" );

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
            } );

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

        // Pick the base (source-language) file for a translation group:
        //   1. Explicit --basic-lan match (by full culture name or 2-letter ISO)
        //   2. The neutral file (no culture in path) — typical .NET satellite layout
        //   3. The English file
        //   4. First alphabetical by culture name
        //   5. None — single-file group or no cultures detected → no NotTranslated checks
        private static FileInfo PickBaseFile( List<GroupMember> files, string explicitBasicLang )
        {
            if ( files.Count < 2 ) return null;

            if ( !string.IsNullOrWhiteSpace( explicitBasicLang ) )
            {
                var explicitMatch = files.FirstOrDefault( f =>
                    f.Culture != null &&
                    ( string.Equals( f.Culture.Name, explicitBasicLang, StringComparison.OrdinalIgnoreCase ) ||
                      string.Equals( f.Culture.TwoLetterISOLanguageName, explicitBasicLang, StringComparison.OrdinalIgnoreCase ) ) );
                if ( explicitMatch != null ) return explicitMatch.FileInfo;
            }

            var neutral = files.FirstOrDefault( f => f.Culture == null );
            if ( neutral != null ) return neutral.FileInfo;

            var english = files.FirstOrDefault( f =>
                f.Culture != null &&
                string.Equals( f.Culture.TwoLetterISOLanguageName, "en", StringComparison.OrdinalIgnoreCase ) );
            if ( english != null ) return english.FileInfo;

            return files
                .Where( f => f.Culture != null )
                .OrderBy( f => f.Culture.Name, StringComparer.OrdinalIgnoreCase )
                .FirstOrDefault()
                ?.FileInfo;
        }

        private sealed class GroupMember
        {
            public FileInfo FileInfo { get; }
            public List<ResourceElement> Elements { get; }
            public CultureInfo Culture { get; }

            public GroupMember( FileInfo fileInfo, List<ResourceElement> elements, CultureInfo culture )
            {
                FileInfo = fileInfo;
                Elements = elements;
                Culture = culture;
            }
        }
    }
}
