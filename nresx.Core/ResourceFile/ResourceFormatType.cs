namespace nresx.Tools
{
    /// <summary>
    /// Identifies a supported resource file format. Each value corresponds to a registered
    /// <see cref="Formatters.FormatRegistry"/> entry with a canonical file extension.
    /// </summary>
    public enum ResourceFormatType
    {
        /// <summary>Not applicable / not specified. Used for unsaved blank instances.</summary>
        NA = 0x00,

        /// <summary>.NET <c>.resx</c> XML format.</summary>
        Resx = 0x01,
        /// <summary>UWP/WinRT <c>.resw</c> format (same shape as .resx).</summary>
        Resw = 0x02,

        /// <summary>YAML, <c>.yml</c> extension.</summary>
        Yml = 0x03,
        /// <summary>YAML, <c>.yaml</c> extension.</summary>
        Yaml = 0x04,

        /// <summary>JSON, <c>.json</c>.</summary>
        Json = 0x05,

        /// <summary>Flat <c>key=value</c> text file, <c>.txt</c>.</summary>
        PlainText = 0x06,
        /// <summary>GNU gettext <c>.po</c>.</summary>
        Po = 0x07,

        /// <summary>XLIFF 1.2, <c>.xlf</c> extension.</summary>
        Xlf = 0x08,
        /// <summary>XLIFF 1.2, <c>.xliff</c> extension.</summary>
        Xliff = 0x09,

        /// <summary>Android <c>strings.xml</c> (<c>&lt;resources&gt;</c> root, <c>&lt;string name="..."&gt;</c> entries).</summary>
        AndroidStrings = 0x0A,
        /// <summary>iOS <c>.strings</c> (<c>"key" = "value";</c> syntax).</summary>
        IosStrings = 0x0B,
        /// <summary>Java <c>.properties</c> (<c>key=value</c>, <c>\uXXXX</c> escapes).</summary>
        JavaProperties = 0x0C,
        /// <summary>INI <c>.ini</c> (<c>key=value</c>, optional <c>[sections]</c>).</summary>
        Ini = 0x0D,

        /// <summary>Comma-separated values <c>.csv</c> (translator exchange).</summary>
        Csv = 0x0E,
        /// <summary>Tab-separated values <c>.tsv</c> (translator exchange).</summary>
        Tsv = 0x0F,

        /// <summary>Flutter Application Resource Bundle <c>.arb</c> (JSON with optional <c>@key</c> metadata).</summary>
        Arb = 0x10,
    }
}