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
        /// <summary>Returns the library version string (e.g. <c>v1.0.0</c>).</summary>
        public static string GetVersion()
        {
            var assembly = Assembly.GetAssembly( typeof( ResourceManager  ) );
            var ver = assembly.GetName().Version;
            var version = $"v{ver.Major}.{ver.Minor}.{ver.Revision}";

            return version;
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
