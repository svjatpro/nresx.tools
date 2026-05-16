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
}
