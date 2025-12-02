using System.Collections.Generic;
using System.Linq;

namespace nresx.Tools
{
    public class ResourceElement
    {
        public ResourceElementType Type { get; set; }
        public string Key { get; set; } = null!;
        public string? KeyPlural { get; set; }
        public string Value { get; set; } = string.Empty;
        public Dictionary<int, string> ValuePlurals { get; set; } = new();

        public string? Comment
        {
            get
            {
                return Comments.FirstOrDefault( c => c.Type == CommentType.Translator )?.Value;
            }
            set
            {
                if ( value == null ) return;
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

        public string GetValue( int plural = -1 )
        {
            if ( plural == -1 ) 
                return Value;
            if ( ValuePlurals.TryGetValue( plural, out var pluralValue ) )
                return pluralValue;

            return Value ?? string.Empty;
        }
    }
}