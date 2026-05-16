#nullable enable

namespace nresx.Tools
{
    // Base for format-specific options.
    // Empty by design — anything format-agnostic that turns out to be needed
    // (validation modes, error policy, etc.) belongs on RSX-126's API, not here.
    public class ResourceFileOption
    {
    }

    public class ResourceFileOptionJson : ResourceFileOption
    {
        // Optional dotted path inside the JSON document where elements live
        // (e.g. "messages.en"). Null means "auto-detect / document root".
        public string? Path { get; set; }

        // Optional property names used when reading/writing element members.
        // Null means "auto-detect from a set of common names" (see FileFormatterJson).
        public string? KeyName { get; set; }
        public string? ValueName { get; set; }
        public string? CommentName { get; set; }

        public Formatters.JsonElementType ElementType { get; set; }
    }

    public class ResourceFileOptionPo : ResourceFileOption
    {
        // If true, PO headers with empty values are dropped when loading.
        public bool IgnoreEmptyHeaders { get; set; } = true;
    }

    public class ResourceFileOptionResx : ResourceFileOption
    {
        // If true (default), writes the multi-line Microsoft commentary at the top
        // of the resx file. Turn off for diff-friendly / compact output.
        public bool WriteRootComment { get; set; } = true;

        // If true (default), writes the embedded XSD schema (~50 lines).
        // Turn off for compact output; .NET ResXResourceReader still reads the file fine.
        public bool WriteEmbeddedSchema { get; set; } = true;

        // If true (default), writes the four standard <resheader> entries
        // (resmimetype, version, reader, writer). Required for .NET ResXResourceReader
        // compatibility — turn off only if writing a "bare" resx for a custom reader.
        public bool WriteStandardResHeaders { get; set; } = true;
    }
}
