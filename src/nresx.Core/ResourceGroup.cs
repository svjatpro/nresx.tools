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
    /// grouping heuristic: (1) files sharing a folder, format, and each carrying a culture extracted
    /// from their file name are grouped together; (2) remaining files are grouped by culture-specific
    /// sibling folders (grouped by grandparent directory); (3) anything left over becomes a
    /// single-file group. Every file is loaded (in <see cref="LoadMode.Lenient"/> mode, so content
    /// validation findings never throw).
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
        foreach ( var path in paths )
        {
            var fileInfo = new FileInfo( path );
            if ( !fileInfo.Exists )
                throw new FileNotFoundException( $"Resource file '{path}' was not found.", path );

            ResourceFormatHelper.DetectFormatByExtension( fileInfo.FullName, out var format );
            fileInfo.FullName.TryToExtractCultureFromPath( out var culture );
            resFiles.Add( (fileInfo.FullName, format, culture) );
        }

        var rawGroups = new List<List<(string fullName, ResourceFormatType format, CultureInfo? culture)>>();

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
