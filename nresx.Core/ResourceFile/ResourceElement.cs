using System.Collections.Generic;
using System.Linq;

namespace nresx.Tools
{
    public class ResourceElement
    {
        public ResourceElementType Type { get; set; }
        public string Key { get; set; } = null!;
        public string Value { get; set; } = string.Empty;

        public string? Comment
        {
            get
            {
                return Comments.FirstOrDefault( c => c.Type == CommentType.Translator )?.Value;
            }
            set
            {
                if ( Comments.All( c => c.Type != CommentType.Translator ) )
                {
                    // implement set null scenario
                    Comments.Add( new Comment( CommentType.Translator, value ) );
                }
                else
                {
                    var comment = Comments.First( c => c.Type == CommentType.Translator );
                    comment.Value = value;
                }
            }
        }

        public readonly List<Comment> Comments = [];
    }
}