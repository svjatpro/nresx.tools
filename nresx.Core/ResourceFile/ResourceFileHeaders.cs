namespace nresx.Tools
{
    /// <summary>
    /// Well-known header keys used inside <see cref="ResourceFile.Headers"/>.
    /// Add new entries here rather than scattering string literals across the codebase.
    /// </summary>
    public static class ResourceFileHeaders
    {
        /// <summary>Standard <c>Language</c> header (BCP 47 code), used by PO/JSON/YAML files and by nresx to derive <see cref="ResourceFile.Culture"/>.</summary>
        public const string Language = "Language";
    }
}
