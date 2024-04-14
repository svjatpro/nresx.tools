using nresx.Tools.Formatters;

namespace nresx.Tools
{
    public class ResourceFileOption
    {

    }

    public class ResourceFileOptionJson : ResourceFileOption
    {
        public string Path { get; set; }

        public string KeyName { get; set; }
        public string ValueName { get; set; }
        public string CommentName { get; set; }

        public JsonElementType ElementType { get; set; }
    }
}
