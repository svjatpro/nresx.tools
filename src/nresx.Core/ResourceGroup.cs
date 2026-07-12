using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using nresx.Core.Extensions;
using nresx.Core.Helpers;

namespace nresx.Core;

/// <summary>
/// A set of related locale files that belong together as one translation unit
/// (e.g. <c>strings.resx</c>, <c>strings_de.resx</c>, <c>strings_fr.resx</c>), together with the
/// base (source-language) file when one can be detected. Use <see cref="Detect"/> to group an
/// explicit list of file paths the same way the CLI does.
/// </summary>
public class ResourceGroup
{
    /// <summary>The files that make up this group, in detection order.</summary>
    public IReadOnlyList<ResourceFile> Files { get; }

    /// <summary>
    /// The base (source-language) file of the group, or <c>null</c> when no base can be
    /// determined (a single-file group, or a group in which no file carries a detectable culture).
    /// This is always one of the entries in <see cref="Files"/> (by reference) when non-null.
    /// </summary>
    public ResourceFile? BaseFile { get; }

    private ResourceGroup( IReadOnlyList<ResourceFile> files, ResourceFile? baseFile )
    {
        Files = files;
        BaseFile = baseFile;
    }

    /// <summary>
    /// Groups an explicit list of resource file paths into translation groups. Mirrors the CLI's
    /// grouping heuristic: (0) namespace-layout files, whose culture is derived from the containing
    /// directory (<c>locales/&lt;lang&gt;/&lt;ns&gt;.&lt;ext&gt;</c>), pair up by identical file name
    /// across the culture folders, so different namespaces in one language folder stay separate;
    /// (1) files sharing a folder, format, and each carrying a culture extracted from their file
    /// name are grouped together; (2) remaining files are grouped by culture-specific sibling folders
    /// (grouped by grandparent directory); (3) anything left over becomes a single-file group. Every
    /// file is loaded (in <see cref="LoadMode.Lenient"/> mode, so content validation findings never throw).
    /// </summary>
    /// <param name="paths">Explicit file paths. No wildcard expansion is performed - each path must exist.</param>
    /// <param name="baseLanguage">
    /// Optional language code (e.g. <c>en</c> or <c>en-US</c>) that forces the base-file pick. When null,
    /// the base file is auto-detected: neutral (no culture) &gt; English &gt; first alphabetical by culture.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paths"/> is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when any path does not exist.</exception>
    /// <exception cref="Exceptions.UnknownResourceFormatException">Thrown when a path's extension is not a recognized resource format.</exception>
    public static IReadOnlyList<ResourceGroup> Detect( IEnumerable<string> paths, string? baseLanguage = null )
    {
        if ( paths == null ) throw new ArgumentNullException( nameof( paths ) );

        var resFiles = new List<(string fullName, ResourceFormatType format, CultureInfo? culture)>();
        var nsFiles = new List<(string fullName, ResourceFormatType format, CultureInfo? culture)>();
        foreach ( var path in paths )
        {
            var fileInfo = new FileInfo( path );
            if ( !fileInfo.Exists )
                throw new FileNotFoundException( $"Resource file '{path}' was not found.", path );

            ResourceFormatHelper.DetectFormatByExtension( fileInfo.FullName, out var format );
            fileInfo.FullName.TryToExtractCultureFromPath( out var culture, out var cultureFromDir );

            // Namespace layout (locales/<lang>/<ns>.<ext>): the culture comes from the directory, so
            // sibling files in one culture folder are DIFFERENT namespaces, not translations of each
            // other. These are grouped in pass 0 by matching name across culture folders; everything
            // else (satellite pattern, no culture) goes through the folder/grandparent passes.
            if ( culture != null && cultureFromDir && format != ResourceFormatType.NA )
                nsFiles.Add( (fileInfo.FullName, format, culture) );
            else
                resFiles.Add( (fileInfo.FullName, format, culture) );
        }

        var rawGroups = new List<List<(string fullName, ResourceFormatType format, CultureInfo? culture)>>();

        // pass 0: namespace files pair up by (parent-of-culture-dir, format, file name) - common.json
        // in uk/ and en/ join one group; common.json and accounting.json in the same lang dir stay
        // apart. A lone namespace (single-language project) becomes its own single-file group.
        foreach ( var grp in nsFiles.GroupBy( r => (
            parent: Path.GetFullPath( Path.Combine( Path.GetDirectoryName( r.fullName )!, ".." ) ),
            r.format,
            name: Path.GetFileName( r.fullName ) ) ) )
        {
            rawGroups.Add( grp.ToList() );
        }

        // pass 1: files in the same folder, same format, each with a detected culture
        resFiles = GroupBy( resFiles, r => Path.GetDirectoryName( r.fullName ), rawGroups );

        // pass 2: culture specific sibling folders (grouped by grandparent directory)
        if ( resFiles.Any() )
            resFiles = GroupBy( resFiles, r => Path.GetFullPath( Path.Combine( r.fullName, "..", ".." ) ), rawGroups );

        // remaining files become single-file groups
        if ( resFiles.Any() )
            rawGroups.AddRange( resFiles.Select( f => new List<(string, ResourceFormatType, CultureInfo?)> { (f.fullName, f.format, f.culture) } ) );

        var lenient = new ResourceFileOption { LoadMode = LoadMode.Lenient };
        var groups = new List<ResourceGroup>();
        foreach ( var raw in rawGroups )
        {
            var members = raw
                .Select( m => (file: new ResourceFile( m.fullName, lenient ), m.culture) )
                .ToList();

            var baseFile = PickBaseFile( members, baseLanguage );
            groups.Add( new ResourceGroup( members.Select( m => m.file ).ToList(), baseFile ) );
        }

        return groups;
    }

    /// <summary>
    /// Validates the group and returns every finding as data (instead of printing). Combines per-file
    /// element checks (duplicate/empty key, empty value - see <see cref="ResourceFileExtensions.ValidateElements(System.Collections.Generic.IEnumerable{ResourceElement},out System.Collections.Generic.List{ResourceElementError})"/>)
    /// with the group-level checks: <see cref="ResourceElementErrorType.MissedElement"/> (a key present in
    /// another file of the group but absent here) and <see cref="ResourceElementErrorType.NotTranslated"/>
    /// (a non-base file whose value equals the <see cref="BaseFile"/>'s value for the same key). Elements are
    /// read raw (duplicates preserved) so duplicate-key findings survive.
    /// </summary>
    public IReadOnlyList<ResourceValidationIssue> Validate()
    {
        var issues = new List<ResourceValidationIssue>();

        var members = Files
            .Select( f => (file: f, elements: ResourceFile.LoadRawElements( f.AbsolutePath ).ToList()) )
            .ToList();

        // union-of-keys map: key -> (file -> first value in that file); used for MissedElement / NotTranslated
        var resourceMap = new Dictionary<string, Dictionary<ResourceFile, string>>();
        foreach ( var m in members )
        {
            foreach ( var element in m.elements )
            {
                var key = element.Key ?? string.Empty;
                if ( !resourceMap.TryGetValue( key, out var byFile ) )
                {
                    byFile = new Dictionary<ResourceFile, string>();
                    resourceMap[key] = byFile;
                }
                if ( !byFile.ContainsKey( m.file ) )
                    byFile[m.file] = element.Value;
            }
        }

        foreach ( var m in members )
        {
            m.elements.ValidateElements( out var errors );

            // missed elements: any key present somewhere else in the group but not here
            var missed = resourceMap.Keys.Except( m.elements.Select( el => el.Key ) ).ToList();
            errors.AddRange( missed.Select( el => new ResourceElementError( ResourceElementErrorType.MissedElement, el ) ) );

            // not translated: only for non-base files, only against the base's value
            if ( BaseFile != null && !ReferenceEquals( m.file, BaseFile ) )
            {
                foreach ( var el in m.elements )
                {
                    if ( !resourceMap.TryGetValue( el.Key, out var elMap ) ) continue;
                    if ( elMap.TryGetValue( BaseFile, out var baseValue ) && baseValue == el.Value )
                        errors.Add( new ResourceElementError( ResourceElementErrorType.NotTranslated, el.Key ) );
                }
            }

            foreach ( var error in errors )
                issues.Add( new ResourceValidationIssue( m.file.AbsolutePath, error ) );
        }

        return issues;
    }

    // Runs one grouping pass over the not-yet-grouped files: buckets by keySelector, and within each
    // bucket by format, promoting any bucket with 2+ culture-carrying files to its own group. Returns
    // the files that were not grouped in this pass (to feed the next pass).
    private static List<(string fullName, ResourceFormatType format, CultureInfo? culture)> GroupBy(
        List<(string fullName, ResourceFormatType format, CultureInfo? culture)> files,
        Func<(string fullName, ResourceFormatType format, CultureInfo? culture), string?> keySelector,
        List<List<(string fullName, ResourceFormatType format, CultureInfo? culture)>> groups )
    {
        return files
            .GroupBy( keySelector )
            .SelectMany( grp =>
            {
                var notProcessed = new List<(string fullName, ResourceFormatType format, CultureInfo? culture)>();
                foreach ( var formatGroup in grp.GroupBy( f => f.format ) )
                {
                    var candidates = formatGroup.Where( f => f.format != ResourceFormatType.NA && f.culture != null ).ToList();
                    if ( candidates.Count > 1 )
                    {
                        groups.Add( candidates );
                        notProcessed.AddRange( formatGroup.Where( f => f.culture == null || f.format == ResourceFormatType.NA ) );
                    }
                    else
                    {
                        notProcessed.AddRange( formatGroup );
                    }
                }
                return notProcessed;
            } )
            .ToList();
    }

    // Picks the base (source-language) file for a translation group:
    //   1. Explicit baseLanguage match (by full culture name or 2-letter ISO)
    //   2. The neutral file (no culture detected in path) - typical .NET satellite layout
    //   3. The English file
    //   4. First alphabetical by culture name
    //   5. None - single-file group or no cultures detected
    private static ResourceFile? PickBaseFile(
        List<(ResourceFile file, CultureInfo? culture)> members,
        string? baseLanguage )
    {
        if ( members.Count < 2 ) return null;

        if ( !string.IsNullOrWhiteSpace( baseLanguage ) )
        {
            var explicitMatch = members.FirstOrDefault( m =>
                m.culture != null &&
                ( string.Equals( m.culture.Name, baseLanguage, StringComparison.OrdinalIgnoreCase ) ||
                  string.Equals( m.culture.TwoLetterISOLanguageName, baseLanguage, StringComparison.OrdinalIgnoreCase ) ) );
            if ( explicitMatch.file != null ) return explicitMatch.file;
        }

        var neutral = members.FirstOrDefault( m => m.culture == null );
        if ( neutral.file != null ) return neutral.file;

        var english = members.FirstOrDefault( m =>
            m.culture != null &&
            string.Equals( m.culture.TwoLetterISOLanguageName, "en", StringComparison.OrdinalIgnoreCase ) );
        if ( english.file != null ) return english.file;

        return members
            .Where( m => m.culture != null )
            .OrderBy( m => m.culture!.Name, StringComparer.OrdinalIgnoreCase )
            .FirstOrDefault()
            .file;
    }
}
