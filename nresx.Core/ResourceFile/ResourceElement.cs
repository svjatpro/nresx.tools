using System.Collections.Generic;
using System.Linq;

namespace nresx.Tools
{
    /// <summary>
    /// A single resource entry: key, value, optional context, optional plural variants, optional comments.
    /// Most formats use only <see cref="Key"/> and <see cref="Value"/>; PO uses the full surface.
    /// </summary>
    public class ResourceElement
    {
        /// <summary>Element kind. Currently <see cref="ResourceElementType.String"/> for all formats.</summary>
        public ResourceElementType Type { get; set; }

        /// <summary>Element identifier (the resource name).</summary>
        public string Key { get; set; } = null!;

        /// <summary>Optional disambiguating context (e.g. PO <c>msgctxt</c>). Most formats ignore this.</summary>
        public string? Context { get; set; }

        /// <summary>Plural form of <see cref="Key"/> (e.g. PO <c>msgid_plural</c>). Null for formats that do not model plurals.</summary>
        public string? KeyPlural { get; set; }

        /// <summary>The primary translated value (or the source string for the neutral file).</summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>Indexed plural translations (e.g. PO <c>msgstr[0]</c>, <c>msgstr[1]</c>, …). Empty for singular-only entries.</summary>
        public Dictionary<int, string> ValuePlurals { get; set; } = new();

        /// <summary>
        /// Convenience accessor for the translator comment (the most common case).
        /// Reads/writes the first <see cref="CommentType.Translator"/> entry in <see cref="Comments"/>;
        /// setting to <c>null</c> is a no-op (use the <see cref="Comments"/> list to remove).
        /// </summary>
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

        /// <summary>All comments attached to this element (translator, extracted, reference, flags, previous-value).</summary>
        public readonly List<Comment> Comments = [];

        /// <summary>
        /// Returns the singular <see cref="Value"/> when <paramref name="plural"/> is <c>-1</c>,
        /// otherwise looks up the corresponding entry in <see cref="ValuePlurals"/>;
        /// falls back to <see cref="Value"/> when no plural variant is registered.
        /// </summary>
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