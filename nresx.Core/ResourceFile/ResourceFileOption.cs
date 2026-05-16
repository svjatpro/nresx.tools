#nullable enable

namespace nresx.Tools
{
    /// <summary>
    /// Base type for format-specific options. Empty by design — pass a derived type
    /// (e.g. <see cref="ResourceFileOptionJson"/>) to a <see cref="ResourceFile"/> constructor or save method
    /// when you need to tune that format's behavior.
    /// </summary>
    public class ResourceFileOption
    {
    }

    /// <summary>JSON-specific load/save options (RSX-120).</summary>
    public class ResourceFileOptionJson : ResourceFileOption
    {
        /// <summary>Optional dotted path inside the JSON document where elements live (e.g. <c>messages.en</c>). Null means "auto-detect / document root".</summary>
        public string? Path { get; set; }

        /// <summary>Optional property name read/written for the element key. Null means "auto-detect from a set of common names".</summary>
        public string? KeyName { get; set; }
        /// <summary>Optional property name read/written for the element value. Null means "auto-detect from a set of common names".</summary>
        public string? ValueName { get; set; }
        /// <summary>Optional property name read/written for the element comment. Null means "auto-detect from a set of common names".</summary>
        public string? CommentName { get; set; }

        /// <summary>JSON shape this options instance is intended for (used to disambiguate ambiguous inputs).</summary>
        public Formatters.JsonElementType ElementType { get; set; }
    }

    /// <summary>PO-specific load/save options.</summary>
    public class ResourceFileOptionPo : ResourceFileOption
    {
        /// <summary>If true (default), PO headers with empty values are dropped when loading.</summary>
        public bool IgnoreEmptyHeaders { get; set; } = true;
    }

    /// <summary>ResX-specific save options. All default to <c>true</c> for parity with the legacy writer (RSX-117).</summary>
    public class ResourceFileOptionResx : ResourceFileOption
    {
        /// <summary>If true (default), writes the multi-line Microsoft commentary at the top of the resx file. Turn off for diff-friendly / compact output.</summary>
        public bool WriteRootComment { get; set; } = true;

        /// <summary>If true (default), writes the embedded XSD schema (~50 lines). Turn off for compact output; .NET <c>ResXResourceReader</c> still reads the file fine without it.</summary>
        public bool WriteEmbeddedSchema { get; set; } = true;

        /// <summary>If true (default), writes the four standard <c>&lt;resheader&gt;</c> entries (resmimetype, version, reader, writer). Required for .NET <c>ResXResourceReader</c> compatibility — turn off only if writing a "bare" resx for a custom reader.</summary>
        public bool WriteStandardResHeaders { get; set; } = true;
    }
}
