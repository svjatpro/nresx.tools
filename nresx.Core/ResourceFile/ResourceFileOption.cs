namespace nresx.Core
{
    /// <summary>
    /// Controls how a <see cref="ResourceFile"/> reacts to per-element parse anomalies and post-load validation findings.
    /// Structural parse errors (malformed file) always throw regardless of mode.
    /// </summary>
    public enum LoadMode
    {
        /// <summary>Default. Validation runs after load; if any Error-severity findings are present, throws <see cref="Exceptions.ValidationException"/>.</summary>
        Strict,

        /// <summary>Validation runs after load; findings are collected on <see cref="ResourceFile.ValidationErrors"/> without throwing.</summary>
        Lenient,

        /// <summary>Skip validation entirely. <see cref="ResourceFile.ValidationErrors"/> stays empty. Closest to a bulk dump.</summary>
        Raw,
    }

    /// <summary>
    /// Base type for format-specific options. Carries the shared <see cref="LoadMode"/> knob;
    /// derived types add format-specific settings (see <see cref="ResourceFileOptionJson"/>, <see cref="ResourceFileOptionPo"/>, …).
    /// </summary>
    public class ResourceFileOption
    {
        /// <summary>How the file's loader should treat validation findings. Defaults to <see cref="LoadMode.Strict"/>.</summary>
        public LoadMode LoadMode { get; set; } = LoadMode.Strict;
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
