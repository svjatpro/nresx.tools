using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nresx.Core.Formatters
{
    // Describes a single resource format: its enum type, file extension,
    // and a factory that builds a formatter instance honoring caller-supplied options.
    internal sealed class FormatDescriptor
    {
        public ResourceFormatType Type { get; }
        public string Extension { get; }
        public Func<ResourceFileOption?, IFileFormatter> CreateFormatter { get; }

        public FormatDescriptor(
            ResourceFormatType type,
            string extension,
            Func<ResourceFileOption?, IFileFormatter> createFormatter )
        {
            Type = type;
            Extension = extension;
            CreateFormatter = createFormatter;
        }
    }

    // Single source of truth for the format → extension → formatter mapping.
    // Built-in formats register themselves at static initialization. New formats
    // (RSX-129 XLIFF onward) plug in by calling Register from the static ctor.
    //
    // Today the registry is internal — public registration is gated on
    // making IFileFormatter public, which is a deliberate future decision.
    internal static class FormatRegistry
    {
        private static readonly List<FormatDescriptor> Descriptors = [];

        static FormatRegistry()
        {
            Register( new FormatDescriptor( ResourceFormatType.Resx,      ".resx", _ => new FileFormatterResx() ) );
            Register( new FormatDescriptor( ResourceFormatType.Resw,      ".resw", _ => new FileFormatterResx() ) );
            Register( new FormatDescriptor( ResourceFormatType.Yml,       ".yml",  _ => new FileFormatterYaml() ) );
            Register( new FormatDescriptor( ResourceFormatType.Yaml,      ".yaml", _ => new FileFormatterYaml() ) );
            Register( new FormatDescriptor( ResourceFormatType.PlainText, ".txt",  _ => new FileFormatterPlainText() ) );
            Register( new FormatDescriptor( ResourceFormatType.Po,        ".po",   options => new FileFormatterPo( options ) ) );
            Register( new FormatDescriptor( ResourceFormatType.Json,      ".json", options => new FileFormatterJson( options ) ) );
            Register( new FormatDescriptor( ResourceFormatType.Xlf,       ".xlf",  _ => new FileFormatterXliff() ) );
            Register( new FormatDescriptor( ResourceFormatType.Xliff,     ".xliff",_ => new FileFormatterXliff() ) );
            Register( new FormatDescriptor( ResourceFormatType.AndroidStrings, ".xml", _ => new FileFormatterAndroidStrings() ) );
            Register( new FormatDescriptor( ResourceFormatType.IosStrings, ".strings", _ => new FileFormatterIosStrings() ) );
            Register( new FormatDescriptor( ResourceFormatType.JavaProperties, ".properties", _ => new FileFormatterJavaProperties() ) );
            Register( new FormatDescriptor( ResourceFormatType.Ini, ".ini", _ => new FileFormatterIni() ) );
            Register( new FormatDescriptor( ResourceFormatType.Csv, ".csv", _ => new FileFormatterCsv( ',' ) ) );
            Register( new FormatDescriptor( ResourceFormatType.Tsv, ".tsv", _ => new FileFormatterCsv( '\t' ) ) );
            Register( new FormatDescriptor( ResourceFormatType.Arb, ".arb", _ => new FileFormatterArb() ) );
        }

        internal static void Register( FormatDescriptor descriptor )
        {
            if ( descriptor == null ) throw new ArgumentNullException( nameof( descriptor ) );
            Descriptors.Add( descriptor );
        }

        internal static bool TryGetByExtension( string path, out FormatDescriptor? descriptor )
        {
            descriptor = null;
            if ( string.IsNullOrWhiteSpace( path ) ) return false;

            var ext = Path.GetExtension( path );
            if ( string.IsNullOrWhiteSpace( ext ) ) return false;

            descriptor = Descriptors.FirstOrDefault(
                d => string.Equals( d.Extension, ext, StringComparison.OrdinalIgnoreCase ) );
            return descriptor != null;
        }

        internal static bool TryGetByType( ResourceFormatType type, out FormatDescriptor? descriptor )
        {
            descriptor = Descriptors.FirstOrDefault( d => d.Type == type );
            return descriptor != null;
        }

        internal static bool TryGetByStream( Stream stream, out FormatDescriptor? descriptor )
        {
            descriptor = null;
            var name = ( stream as FileStream )?.Name;
            return !string.IsNullOrWhiteSpace( name ) && TryGetByExtension( name!, out descriptor );
        }
    }
}
