namespace nresx.Tools
{
    public class ResourceElement
    {
        public ResourceElementType Type { get; set; }
        public string Key { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }
}