using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using nresx.CommandLine.CodeParsers;
using nresx.Core;
using nresx.Core.Extensions;
using nresx.Core.Helpers;

namespace nresx.CommandLine.Analysis
{
    // Zero-config project scan behind a bare `nresx validate` (RSX-250, RSX-264). Walks a
    // directory tree once (cheap), decides whether the project itself is localized, and produces
    // a ProjectAnalysis. Localization state and resource presence are kept as two independent
    // facts: resource files that live only under test/fixture folders do NOT make a project
    // "localized" (RSX-264). For a localized project it groups the locale files (namespace-aware,
    // RSX-252) and validates them; otherwise it scans the source and estimates extractable tokens.
    public class ProjectAnalyzer
    {
        // Directories that never hold hand-authored localization and only add noise.
        private static readonly HashSet<string> SkipDirs = new( StringComparer.OrdinalIgnoreCase )
        {
            ".git", ".vs", ".svn", ".hg", ".idea", ".vscode",
            "bin", "obj", "node_modules", "packages", "dist", "build", ".test_output"
        };

        // Path segments that mark test/sample/fixture material - resources found only here belong
        // to the test suite or samples, not to the project's own localization (RSX-264).
        private static readonly HashSet<string> FixtureDirNames = new( StringComparer.OrdinalIgnoreCase )
        {
            "test", "tests", "testing", "__tests__", "test_files", "test_projects",
            "testdata", "testfiles", "fixture", "fixtures", "sample", "samples",
            "example", "examples", "spec", "specs", "mock", "mocks", "demo", "demos"
        };

        // Extensions that unambiguously mean "localization file" - counted wherever they sit.
        private static readonly HashSet<string> StrongLocExtensions = new( StringComparer.OrdinalIgnoreCase )
        {
            ".resx", ".resw", ".po", ".arb", ".xlf", ".xliff", ".strings"
        };

        // Extensions that are ALSO used heavily for config/build/data - a lone one is not
        // localization (osbb `version.properties`, a stray culture-suffixed `.txt`). They count
        // only with corroboration: a locale-ish folder, or a real culture group (RSX-264).
        private static readonly HashSet<string> WeakLocExtensions = new( StringComparer.OrdinalIgnoreCase )
        {
            ".properties", ".json", ".yaml", ".yml", ".ini", ".csv", ".tsv", ".txt", ".xlsx"
        };

        // Folder names that mark a localization tree, qualifying a weak-format file on their own.
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

        // High-precision resource-consumption patterns (RSX-264). Absence never implies "orphan"
        // (a designer-accessor app matches none of these), so this only UPGRADES confidence.
        private static readonly Regex CsGetString = new( @"\.GetString\s*\(", RegexOptions.Compiled );
        private static readonly Regex CsLocalizer = new( @"IStringLocalizer|\b_?[Ll]ocalizer\s*\[", RegexOptions.Compiled );
        private static readonly Regex XamlReference = new( @"x:Uid\s*=|\{\s*x:Static[^}]*Resources", RegexOptions.Compiled );

        // Per-issue lines (and resource-file paths) are listed individually up to this count.
        public const int IssueListLimit = 10;

        // Plain validate is budgeted so it never hangs on a large repo (RSX-264). Pass
        // ScanBudget.Unlimited for an exhaustive scan.
        public ProjectAnalysis Analyze( string root, ScanBudget budget = null )
        {
            budget ??= new ScanBudget();
            var clock = Stopwatch.StartNew();
            var analysis = new ProjectAnalysis { Root = root };

            var allFiles = EnumerateFiles( root ).ToList();

            // Weak-format corroboration needs to know the sibling set, so classify up front.
            var weakGroups = BuildWeakCultureGroups( allFiles );
            var locFiles = allFiles.Where( f => IsLocalizationResource( f, weakGroups ) ).ToList();

            var projectLoc = locFiles.Where( f => !IsFixturePath( f ) ).ToList();
            var fixtureLoc = locFiles.Where( IsFixturePath ).ToList();
            var sourceFiles = allFiles
                .Where( f => SourceParsers.ContainsKey( Path.GetExtension( f ) ) && !IsFixturePath( f ) )
                .ToList();

            if ( projectLoc.Count > 0 )
            {
                AnalyzeLocalized( analysis, projectLoc, sourceFiles, budget, clock );
                return analysis;
            }

            // Not localized. Record any fixture-only resources as a separate fact, scan the source.
            analysis.Localized = false;
            if ( fixtureLoc.Count > 0 )
            {
                analysis.FixtureResourceFileCount = fixtureLoc.Count;
                analysis.FixtureFormats.AddRange( DistinctExtensions( fixtureLoc ) );
            }

            AnalyzeNotLocalized( analysis, sourceFiles, budget, clock );

            analysis.NoProject = fixtureLoc.Count == 0 && sourceFiles.Count == 0;
            return analysis;
        }

        private void AnalyzeLocalized( ProjectAnalysis analysis, List<string> localeFiles,
            List<string> sourceFiles, ScanBudget budget, Stopwatch clock )
        {
            analysis.Localized = true;
            analysis.ResourceFileCount = localeFiles.Count;

            analysis.Formats.AddRange( DistinctExtensions( localeFiles ) );
            analysis.Languages.AddRange( localeFiles
                .Select( f => f.TryToExtractCultureFromPath( out var c ) ? c.Name : null )
                .Where( c => c != null )
                .Distinct()
                .OrderBy( c => c, StringComparer.OrdinalIgnoreCase ) );

            // List the resource files themselves when there are few (RSX-264: single/short projects
            // must show which file was found, not just a count).
            if ( localeFiles.Count <= IssueListLimit )
                analysis.ResourceFilePaths.AddRange( localeFiles.Select( f => f.GetShortPath() ) );

            // Files with a known extension that still fail to load are reported, never silently
            // dropped (RSX-245 B3). Detect.Validate loads leniently, so probe loadability here.
            // Bounded by the scan budget so a huge repo does not hang the plain run (RSX-264).
            var loadable = new List<string>();
            var scanned = 0;
            foreach ( var file in localeFiles )
            {
                if ( scanned >= budget.MaxResourceFiles || clock.Elapsed > budget.MaxDuration )
                    break;
                scanned++;
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
            analysis.ResourceFilesScanned = scanned;
            analysis.ResourceScanPartial = scanned < localeFiles.Count;

            var groups = ResourceGroup.Detect( loadable );
            analysis.GroupCount = groups.Count;
            analysis.Layout = DescribeLayout( loadable );
            analysis.SourceReferencesResources = SourceReferencesResources( sourceFiles, budget, clock );

            foreach ( var group in groups )
            {
                if ( clock.Elapsed > budget.MaxDuration )
                {
                    analysis.ResourceScanPartial = true;
                    break;
                }
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

        private void AnalyzeNotLocalized( ProjectAnalysis analysis, List<string> sourceFiles,
            ScanBudget budget, Stopwatch clock )
        {
            analysis.SourceFileCount = sourceFiles.Count;
            analysis.SourceBytes = sourceFiles.Sum( SafeLength );

            if ( sourceFiles.Count == 0 )
                return;

            // Extract tokens until any budget cap is hit, then report the result as partial so a
            // large codebase never hangs the plain run (RSX-264).
            var tokens = new HashSet<string>();
            var scanned = 0;
            long bytes = 0;
            foreach ( var file in sourceFiles )
            {
                if ( scanned >= budget.MaxSourceFiles || bytes > budget.MaxSourceBytes ||
                     clock.Elapsed > budget.MaxDuration )
                    break;
                scanned++;
                bytes += SafeLength( file );
                CountTokens( file, tokens );
            }

            analysis.SourceFilesScanned = scanned;
            analysis.SourceScanPartial = scanned < sourceFiles.Count;
            analysis.PotentialTokens = tokens.Count;
        }

        // Conservative confidence signal (RSX-264): does source visibly consume localized resources?
        // Kept precise enough to be meaningful; a false negative only omits an informational note.
        // Bounded by the same budget so it cannot hang on a large source tree.
        private static bool SourceReferencesResources( List<string> sourceFiles, ScanBudget budget, Stopwatch clock )
        {
            var scanned = 0;
            foreach ( var file in sourceFiles )
            {
                if ( scanned >= budget.MaxSourceFiles || clock.Elapsed > budget.MaxDuration )
                    break;
                scanned++;

                string text;
                try { text = File.ReadAllText( file ); }
                catch { continue; }

                var ext = Path.GetExtension( file );
                if ( ext.Equals( ".xaml", StringComparison.OrdinalIgnoreCase ) )
                {
                    if ( XamlReference.IsMatch( text ) )
                        return true;
                }
                else if ( CsLocalizer.IsMatch( text ) ||
                          ( text.Contains( "ResourceManager" ) && CsGetString.IsMatch( text ) ) )
                {
                    return true;
                }
            }

            return false;
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

        // Groups weak-format files by (directory, extension, culture-stripped base) so a real
        // translation set (>=2 files, at least one carrying a culture) can be told apart from a
        // lone config/build file that merely shares the extension.
        private static HashSet<string> BuildWeakCultureGroups( List<string> allFiles )
        {
            var byKey = new Dictionary<string, (int count, bool anyCulture)>();
            foreach ( var file in allFiles )
            {
                var ext = Path.GetExtension( file );
                if ( !WeakLocExtensions.Contains( ext ) )
                    continue;

                var key = WeakGroupKey( file );
                byKey.TryGetValue( key, out var v );
                var cultured = file.TryToExtractCultureFromPath( out _ );
                byKey[key] = ( v.count + 1, v.anyCulture || cultured );
            }

            return byKey
                .Where( kv => kv.Value.count >= 2 && kv.Value.anyCulture )
                .Select( kv => kv.Key )
                .ToHashSet( StringComparer.OrdinalIgnoreCase );
        }

        private static string WeakGroupKey( string path )
        {
            var name = Path.GetFileNameWithoutExtension( path ) ?? string.Empty;
            if ( path.TryToExtractCultureFromPath( out var culture, out var fromDir ) && !fromDir )
            {
                foreach ( var sep in new[] { '.', '_', '-' } )
                {
                    var suffix = sep + culture.Name;
                    if ( name.EndsWith( suffix, StringComparison.OrdinalIgnoreCase ) )
                    {
                        name = name.Substring( 0, name.Length - suffix.Length );
                        break;
                    }
                }
            }

            var dir = Path.GetDirectoryName( path ) ?? string.Empty;
            var ext = Path.GetExtension( path ) ?? string.Empty;
            return $"{dir.ToLowerInvariant()}|{ext.ToLowerInvariant()}|{name.ToLowerInvariant()}";
        }

        private static bool IsLocalizationResource( string path, HashSet<string> weakGroups )
        {
            if ( !ResourceFormatHelper.DetectFormatByExtension( path, out _ ) )
                return false;

            var ext = Path.GetExtension( path );
            if ( StrongLocExtensions.Contains( ext ) )
                return true;
            if ( !WeakLocExtensions.Contains( ext ) )
                return false;

            // Weak format: needs corroboration. A locale-ish folder qualifies on its own; otherwise
            // it must be part of a real culture group (see BuildWeakCultureGroups). A random
            // appsettings.json / version.properties / stray .txt satisfies neither (RSX-264).
            if ( IsUnderDir( path, LocaleDirNames ) )
                return true;
            return weakGroups.Contains( WeakGroupKey( path ) );
        }

        private static bool IsFixturePath( string path ) => IsUnderDir( path, FixtureDirNames );

        private static bool IsUnderDir( string path, HashSet<string> dirNames )
        {
            var dir = Path.GetDirectoryName( path );
            if ( string.IsNullOrEmpty( dir ) )
                return false;
            var segments = dir.Split( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar );
            // Match a leading-dot variant too (`.test_files` -> `test_files`) so hidden fixture
            // folders are recognized without listing every dotted spelling.
            return segments.Any( s => dirNames.Contains( s ) || dirNames.Contains( s.TrimStart( '.' ) ) );
        }

        private static IEnumerable<string> DistinctExtensions( IEnumerable<string> files ) => files
            .Select( f => Path.GetExtension( f ).ToLowerInvariant() )
            .Distinct()
            .OrderBy( e => e, StringComparer.OrdinalIgnoreCase );

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
