using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using nresx.Core.Exceptions;
using nresx.Core.Helpers;

namespace nresx.Core
{
    /// <summary>
    /// Static facade over the top-level, multi-file resource operations (RSX-235): converting a file
    /// between formats, loading and validating translation groups, and diffing two files. Single-file
    /// primitives live on <see cref="ResourceFile"/>; wildcard/glob expansion stays in the CLI - these
    /// methods take explicit paths.
    /// </summary>
    public class ResourceManager
    {
        /// <summary>Returns the library version string (e.g. <c>v1.0.0-beta.2</c>).</summary>
        public static string GetVersion()
        {
            var assembly = Assembly.GetAssembly( typeof( ResourceManager ) );

            // AssemblyInformationalVersion carries the full semver, including any
            // prerelease suffix (e.g. 1.0.0-beta.2). SourceLink may append
            // "+<commit-hash>" build metadata - trim it for display.
            var info = assembly?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;
            if ( !string.IsNullOrEmpty( info ) )
            {
                var plus = info.IndexOf( '+' );
                if ( plus >= 0 ) info = info.Substring( 0, plus );
                return $"v{info}";
            }

            // Fallback: numeric assembly version (no prerelease suffix possible).
            var ver = assembly.GetName().Version;
            return $"v{ver.Major}.{ver.Minor}.{ver.Build}";
        }

        /// <summary>
        /// Groups an explicit list of resource file paths into translation groups (locale sets), detecting
        /// the base/source-language file of each. Wildcard expansion is a CLI concern - pass concrete paths.
        /// Convenience passthrough to <see cref="ResourceGroup.Detect"/>.
        /// </summary>
        /// <param name="paths">Explicit file paths (each must exist).</param>
        /// <param name="baseLanguage">Optional language code (e.g. <c>en</c>) forcing the base-file pick; auto-detected when null.</param>
        /// <exception cref="System.IO.FileNotFoundException">Thrown when any path does not exist.</exception>
        public static IReadOnlyList<ResourceGroup> LoadGroups( IEnumerable<string> paths, string? baseLanguage = null )
        {
            return ResourceGroup.Detect( paths, baseLanguage );
        }

        /// <summary>
        /// Validates a set of resource files as translation groups and returns every finding as data.
        /// Files are grouped by <see cref="ResourceGroup.Detect"/>, then each group is validated
        /// (per-file element checks plus group-level missed / not-translated checks - see
        /// <see cref="ResourceGroup.Validate"/>).
        /// </summary>
        /// <param name="paths">Explicit file paths (each must exist).</param>
        /// <param name="baseLanguage">Optional language code (e.g. <c>en</c>) forcing the base-file pick; auto-detected when null.</param>
        /// <exception cref="System.IO.FileNotFoundException">Thrown when any path does not exist.</exception>
        public static IReadOnlyList<ResourceValidationIssue> Validate( IEnumerable<string> paths, string? baseLanguage = null )
        {
            return ResourceGroup.Detect( paths, baseLanguage )
                .SelectMany( group => group.Validate() )
                .ToList();
        }

        /// <summary>
        /// Loads <paramref name="sourcePath"/> and writes it to <paramref name="destinationPath"/> in a
        /// (possibly different) format. When <paramref name="format"/> is null the target format is derived
        /// from the destination path's extension.
        /// </summary>
        /// <param name="sourcePath">Path of the file to read. Format is inferred from its extension.</param>
        /// <param name="destinationPath">Path to write. When <paramref name="format"/> is null, its extension selects the target format.</param>
        /// <param name="format">Explicit target format; when null, derived from <paramref name="destinationPath"/>'s extension.</param>
        /// <exception cref="UnknownResourceFormatException">Thrown when the target format is neither supplied nor derivable from the destination extension (or the source extension is unrecognized).</exception>
        /// <exception cref="ResourceFormatMismatchException">Thrown when an explicit <paramref name="format"/> conflicts with the destination path's extension.</exception>
        public static void Convert( string sourcePath, string destinationPath, ResourceFormatType? format = null )
        {
            var source = new ResourceFile( sourcePath );

            var targetFormat = format ?? ResourceFormatType.NA;
            if ( targetFormat == ResourceFormatType.NA )
                ResourceFormatHelper.DetectFormatByExtension( destinationPath, out targetFormat );
            if ( targetFormat == ResourceFormatType.NA )
                throw new UnknownResourceFormatException();

            source.Save( destinationPath, targetFormat, createDir: false );
        }

        /// <summary>
        /// Computes the key-based difference between two resource files (which may be in different formats).
        /// Elements are read raw (duplicates preserved; first occurrence per key wins). See <see cref="ResourceDiff"/>.
        /// </summary>
        /// <param name="firstPath">The "before" / left file.</param>
        /// <param name="secondPath">The "after" / right file.</param>
        /// <exception cref="UnknownResourceFormatException">Thrown when either path's extension is not a recognized resource format.</exception>
        public static ResourceDiff Diff( string firstPath, string secondPath )
        {
            var first = ResourceFile.LoadRawElements( firstPath );
            var second = ResourceFile.LoadRawElements( secondPath );
            return ResourceDiff.Compare( first, second );
        }
    }
}
