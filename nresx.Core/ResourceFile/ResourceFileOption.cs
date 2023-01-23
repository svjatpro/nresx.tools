using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
