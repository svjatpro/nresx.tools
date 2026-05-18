using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace nresx.Core
{
    /// <summary>
    /// Ordered, mutable collection of <see cref="ResourceElement"/>. Indexable by position,
    /// by <see cref="ResourceElement.Key"/>, or by <c>(key, context)</c> for PO-style disambiguation.
    /// </summary>
    public sealed class ResourceElements( IEnumerable<ResourceElement>? elements = null )
        : IEnumerable<ResourceElement>
    {
        private readonly List<ResourceElement> ElementsList = elements?.ToList() ?? [];

        /// <summary>Access by ordinal position.</summary>
        /// <exception cref="ArgumentNullException">Thrown when assigning <c>null</c>.</exception>
        public ResourceElement? this[int index]
        {
            get => ElementsList[index];
            set => ElementsList[index] = value ?? throw new ArgumentNullException( nameof( value ) );
        }

        /// <summary>Access by key. Setting an unknown key adds a new element; getting an unknown key returns <c>null</c>.</summary>
        /// <exception cref="ArgumentNullException">Thrown when assigning <c>null</c>.</exception>
        public ResourceElement? this[string key]
        {
            get => ElementsList.SingleOrDefault( el => el.Key == key );
            set
            {
                if ( value == null ) throw new ArgumentNullException( nameof( value ) );
                var index = ElementsList.FindIndex( el => el.Key == key );
                if ( index < 0 )
                    ElementsList.Add( value );
                else
                    ElementsList[index] = value;
            }
        }

        /// <summary>Access by (key, context) for formats that distinguish on context (e.g. PO <c>msgctxt</c>).</summary>
        /// <exception cref="ArgumentNullException">Thrown when assigning <c>null</c>.</exception>
        public ResourceElement? this[string key, string? context]
        {
            get => ElementsList.SingleOrDefault( el => el.Key == key && el.Context == context );
            set
            {
                if ( value == null ) throw new ArgumentNullException( nameof( value ) );
                var index = ElementsList.FindIndex( el => el.Key == key && el.Context == context );
                if ( index < 0 )
                    ElementsList.Add( value );
                else
                    ElementsList[index] = value;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<ResourceElement> GetEnumerator()
        {
            return ElementsList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Appends a new string element. Duplicate keys are not checked here — pass through validation
        /// (see <c>Validate</c> extension) to detect duplicates after loading.
        /// </summary>
        public void Add(
            string key,
            string value,
            string? comment = null,
            string? context = null,
            string? keyPlural = null,
            (int, string)[]? plurals = null )
        {
            var el = new ResourceElement
            {
                Type = ResourceElementType.String,
                Key = key,
                KeyPlural = keyPlural,
                Value = value,
                Comment = comment,
                Context = context,
            };
            if ( plurals != null )
            {
                foreach ( var (idx, pluralValue) in plurals )
                    el.SetPlural( idx, pluralValue );
            }

            ElementsList.Add( el );
        }

        /// <summary>Removes the element with the given key.</summary>
        /// <exception cref="KeyNotFoundException">Thrown when no element with that key exists. Use <see cref="TryRemove"/> for no-throw semantics.</exception>
        public void Remove( string key )
        {
            var index = ElementsList.FindIndex( el => el.Key == key );
            if ( index == -1 )
                throw new KeyNotFoundException( $"Element with \"{key}\" key not found" );
            ElementsList.RemoveAt( index );
        }

        /// <summary>Removes the element with the given key, returning <c>false</c> when not found.</summary>
        public bool TryRemove( string key, out ResourceElement? element )
        {
            element = ElementsList.FirstOrDefault( el => el.Key == key );
            if ( element == null ) return false;

            ElementsList.Remove( element );
            return true;
        }
    }
}