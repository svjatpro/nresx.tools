using System;
using System.Collections.Generic;
using System.Linq;

namespace nresx.Core
{
    /// <summary>
    /// A single resource entry: key, value, optional context, optional plural variants, optional comments.
    /// Most formats use only <see cref="Key"/> and <see cref="Value"/>; PO uses the full surface.
    /// </summary>
    public class ResourceElement
    {
        private readonly List<Comment> _comments = new();
        private readonly Dictionary<int, string> _valuePlurals = new();

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

        /// <summary>
        /// Indexed plural translations (e.g. PO <c>msgstr[0]</c>, <c>msgstr[1]</c>, …). Empty for singular-only entries.
        /// Read-only view; use <see cref="SetPlural"/> / <see cref="RemovePlural"/> / <see cref="ClearPlurals"/> to mutate.
        /// </summary>
        public IReadOnlyDictionary<int, string> ValuePlurals => _valuePlurals;

        /// <summary>
        /// Convenience accessor for the translator comment (the most common case).
        /// Reads/writes the first <see cref="CommentType.Translator"/> entry in <see cref="Comments"/>;
        /// setting to <c>null</c> is a no-op (use <see cref="RemoveComment"/> to remove).
        /// </summary>
        public string? Comment
        {
            get => _comments.FirstOrDefault( c => c.Type == CommentType.Translator )?.Value;
            set
            {
                if ( value == null ) return;
                var existing = _comments.FirstOrDefault( c => c.Type == CommentType.Translator );
                if ( existing != null )
                    existing.Value = value;
                else
                    _comments.Add( new Comment( CommentType.Translator, value ) );
            }
        }

        /// <summary>
        /// All comments attached to this element (translator, extracted, reference, flags, previous-value).
        /// Read-only view; use <see cref="AddComment(Comment)"/> / <see cref="RemoveComment"/> / <see cref="ClearComments"/> to mutate.
        /// </summary>
        public IReadOnlyList<Comment> Comments => _comments;

        /// <summary>Appends a comment to <see cref="Comments"/>.</summary>
        public void AddComment( Comment comment )
        {
            if ( comment == null ) throw new ArgumentNullException( nameof( comment ) );
            _comments.Add( comment );
        }

        /// <summary>Convenience overload: creates and appends a <see cref="Comment"/> of the given type and text.</summary>
        public void AddComment( CommentType type, string? value )
        {
            _comments.Add( new Comment( type, value ) );
        }

        /// <summary>Removes the given comment instance. Returns true if it was present.</summary>
        public bool RemoveComment( Comment comment ) => _comments.Remove( comment );

        /// <summary>Removes all comments.</summary>
        public void ClearComments() => _comments.Clear();

        /// <summary>Sets the plural value at the given index (0, 1, 2, …). Replaces any existing entry at that index.</summary>
        public void SetPlural( int index, string value )
        {
            _valuePlurals[index] = value ?? string.Empty;
        }

        /// <summary>Removes the plural at the given index. Returns true if it was present.</summary>
        public bool RemovePlural( int index ) => _valuePlurals.Remove( index );

        /// <summary>Removes all plural values.</summary>
        public void ClearPlurals() => _valuePlurals.Clear();

        /// <summary>
        /// Returns the singular <see cref="Value"/> when <paramref name="plural"/> is <c>-1</c>,
        /// otherwise looks up the corresponding entry in <see cref="ValuePlurals"/>;
        /// falls back to <see cref="Value"/> when no plural variant is registered.
        /// </summary>
        public string GetValue( int plural = -1 )
        {
            if ( plural == -1 )
                return Value;
            if ( _valuePlurals.TryGetValue( plural, out var pluralValue ) )
                return pluralValue;

            return Value ?? string.Empty;
        }
    }
}
