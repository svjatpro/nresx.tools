using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using nresx.CommandLine.CodeParsers;
using nresx.Core;
using nresx.Core.Extensions;
using nresx.Core.Helpers;

namespace nresx.CommandLine.Analysis
{
    // Zero-config project scan behind a bare `nresx validate` (RSX-250). Walks a directory
    // tree once (cheap), decides whether the project is already localized, and produces a
    // ProjectAnalysis. For a localized project it groups the locale files (namespace-aware,
    // RSX-252) and validates them; for a not-yet-localized one it scans the source code and
    // estimates how many user-facing strings could become resource tokens.
    public class ProjectAnalyzer
    {
        // Directories that never hold hand-authored localization and only add noise.
        private static readonly HashSet<string> SkipDirs = new( StringComparer.OrdinalIgnoreCase )
        {
            ".git", ".vs", ".svn", ".hg", ".idea", ".vscode",
            "bin", "obj", "node_modules", "packages", "dist", "build", ".test_output"
        };

        // Extensions that unambiguously mean "localization file" - counted wherever they sit.
        private static readonly HashSet<string> PrimaryLocExtensions = new( StringComparer.OrdinalIgnoreCase )
        {
            ".resx", ".resw", ".po", ".arb", ".xlf", ".xliff", ".strings", ".properties"
        };

        // Folder names that mark a localization tree, qualifying ambiguous formats (.json/.yaml).
        private static readonly HashSet<string> LocaleDirNames = new( StringComparer.OrdinalIgnoreCase )
        {
            "locales", "locale", "i18n", "l10n", "lang", "langs", "languages", "translations", "resources"
        };

        // Source extensions we can extract tokens from (matches GenerateCommand's parsers).
        private static readonly Dictionary<string, Func<ICodeParser>> SourceParsers = new( StringComparer.OrdinalIgnoreCase )
        {
            { ".cs", () => new CsCodeParser() },
            { ".xaml", () => new XamlCodeParser() }
        };

        // Above these sizes the source is "too big" to extract in-line - report the scan only.
        private const int MaxScanFiles = 400;
        private const long MaxScanBytes = 8L * 1024 * 1024;

        // Per-issue lines are listed individually up to this count, then collapsed to totals.
        public const int IssueListLimit = 10;

        public ProjectAnalysis Analyze( string root )
        {
            var analysis = new ProjectAnalysis { Root = root };

            var allFiles = EnumerateFiles( root ).ToList();
            var localeFiles = allFiles.Where( IsLocalizationResource ).ToList();

            if ( localeFiles.Count > 0 )
            {
                AnalyzeLocalized( analysis, localeFiles );
            }
            else
            {
                AnalyzeNotLocalized( analysis, allFiles );
            }

            return analysis;
        }

        private void AnalyzeLocalized( ProjectAnalysis analysis, List<string> localeFiles )
        {
            analysis.Localized = true;
            analysis.ResourceFileCount = localeFiles.Count;

            analysis.Formats.AddRange( localeFiles
                .Select( f => Path.GetExtension( f ).ToLowerInvariant() )
                .Distinct()
                .OrderBy( e => e, StringComparer.OrdinalIgnoreCase ) );

            analysis.Languages.AddRange( localeFiles
                .Select( f => f.TryToExtractCultureFromPath( out var c ) ? c.Name : null )
                .Where( c => c != null )
                .Distinct()
                .OrderBy( c => c, StringComparer.OrdinalIgnoreCase ) );

            // Files with a known extension that still fail to load are reported, never silently
            // dropped (RSX-245 B3). Detect.Validate loads leniently, so probe loadability here.
            var loadable = new List<string>();
            foreach ( var file in localeFiles )
            {
                try
                {
                    _ = new ResourceFile( file, new ResourceFileOption { LoadMode = LoadMode.Lenient } );
                    loadable.Add( file );
                }
                catch
                {
                    analysis.UnreadableFiles.Add( file );
                }
            }

            var groups = ResourceGroup.Detect( loadable );
            analysis.GroupCount = groups.Count;
            analysis.Layout = DescribeLayout( loadable );

            foreach ( var group in groups )
            {
                foreach ( var issue in group.Validate() )
                {
                    var severity = issue.Error.ErrorType.GetSeverity();
                    analysis.TotalIssues++;
                    if ( severity == ResourceElementErrorSeverity.Error ) analysis.Errors++;
                    else analysis.Warnings++;

                    var rule = issue.Error.ErrorType.ToString();
                    analysis.IssuesByRule.TryGetValue( rule, out var count );
                    analysis.IssuesByRule[rule] = count + 1;

                    if ( analysis.TotalIssues <= IssueListLimit )
                    {
                        var label = severity == ResourceElementErrorSeverity.Error ? "error" : "warning";
                        var detail = !string.IsNullOrWhiteSpace( issue.Error.ElementKey )
                            ? issue.Error.ElementKey
                            : ( issue.Error.Message ?? string.Empty );
                        analysis.IssueLines.Add( $"{issue.FilePath.GetShortPath()}: {label}: {rule}: {detail}" );
                    }
                }
            }
        }

        private void AnalyzeNotLocalized( ProjectAnalysis analysis, List<string> allFiles )
        {
            analysis.Localized = false;

            var sourceFiles = allFiles
                .Where( f => SourceParsers.ContainsKey( Path.GetExtension( f ) ) )
                .ToList();

            analysis.SourceFileCount = sourceFiles.Count;
            analysis.SourceBytes = sourceFiles.Sum( SafeLength );

            if ( sourceFiles.Count > MaxScanFiles || analysis.SourceBytes > MaxScanBytes )
            {
                // Too large to extract in-line - report the scan only (user runs `nresx generate`).
                analysis.SourceTooLarge = true;
                return;
            }

            var tokens = new HashSet<string>();
            foreach ( var file in sourceFiles )
                CountTokens( file, tokens );
            analysis.PotentialTokens = tokens.Count;
        }

        // Counts the distinct extractable strings in one source file (dry extraction - no writes).
        private static void CountTokens( string file, HashSet<string> tokens )
        {
            if ( !SourceParsers.TryGetValue( Path.GetExtension( file ), out var factory ) )
                return;
            var parser = factory();
            try
            {
                foreach ( var line in File.ReadLines( file ) )
                {
                    parser.ProcessNextLine( line, "token",
                        ( key, value ) => { tokens.Add( value ); return null; },
                        _ => { } );
                }
            }
            catch
            {
                // a source file we cannot read just contributes no tokens
            }
        }

        // Names the dominant layout for the report: namespace tree, satellite files, or flat.
        private static string DescribeLayout( List<string> files )
        {
            var fromDir = 0;
            var fromName = 0;
            var neutral = 0;
            foreach ( var file in files )
            {
                if ( file.TryToExtractCultureFromPath( out _, out var byDir ) )
                {
                    if ( byDir ) fromDir++;
                    else fromName++;
                }
                else
                {
                    neutral++;
                }
            }

            if ( fromDir > 0 && fromDir >= fromName )
                return "namespace tree (locales/<lang>/<file>)";
            if ( fromName > 0 )
                return "satellite files (name.<lang>.<ext>)";
            return neutral > 0 ? "flat (single language, no culture in paths)" : "mixed";
        }

        private static bool IsLocalizationResource( string path )
        {
            if ( !ResourceFormatHelper.DetectFormatByExtension( path, out _ ) )
                return false;

            var ext = Path.GetExtension( path );
            if ( PrimaryLocExtensions.Contains( ext ) )
                return true;

            // Ambiguous content formats (.json/.yaml/.ini/.csv/...) only count as localization
            // when a culture is detectable or they live under a locale-ish folder - otherwise a
            // random appsettings.json would make every project look "localized".
            if ( path.TryToExtractCultureFromPath( out _ ) )
                return true;

            var dirs = Path.GetDirectoryName( path )?
                .Split( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar ) ?? Array.Empty<string>();
            return dirs.Any( LocaleDirNames.Contains );
        }

        private static long SafeLength( string path )
        {
            try { return new FileInfo( path ).Length; }
            catch { return 0; }
        }

        // Depth-first file walk that prunes the skip dirs (and never throws on an unreadable dir).
        private static IEnumerable<string> EnumerateFiles( string root )
        {
            var stack = new Stack<string>();
            stack.Push( root );
            while ( stack.Count > 0 )
            {
                var dir = stack.Pop();

                string[] subdirs;
                try { subdirs = Directory.GetDirectories( dir ); }
                catch { subdirs = Array.Empty<string>(); }
                foreach ( var sub in subdirs )
                {
                    if ( !SkipDirs.Contains( new DirectoryInfo( sub ).Name ) )
                        stack.Push( sub );
                }

                string[] files;
                try { files = Directory.GetFiles( dir ); }
                catch { files = Array.Empty<string>(); }
                foreach ( var file in files )
                    yield return file;
            }
        }
    }
}
