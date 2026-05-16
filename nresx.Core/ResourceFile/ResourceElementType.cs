namespace nresx.Tools
{
    /// <summary>Kind of value a <see cref="ResourceElement"/> carries. Currently only string is modeled.</summary>
    public enum ResourceElementType
    {
        /// <summary>Unspecified.</summary>
        None = 0x00,
        /// <summary>String value.</summary>
        String = 0x01,
    }
}