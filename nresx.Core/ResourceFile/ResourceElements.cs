using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace nresx.Tools
{
    public sealed class ResourceElements( IEnumerable<ResourceElement>? elements = null )
        : IEnumerable<ResourceElement>
    {
        private readonly List<ResourceElement> ElementsList = elements?.ToList() ?? [];

        public ResourceElement? this[int index]
        {
            get => ElementsList[index];
            set => ElementsList[index] = value ?? throw new NullReferenceException("Cannot set null ResourceElement");
        }

        public ResourceElement? this[string key]
        {
            get => ElementsList.SingleOrDefault( el => el.Key == key );
            set
            {
                var el = ElementsList
                    .Select( ( element, index ) => ( element, index ) )
                    .SingleOrDefault( el => el.element.Key == key );
                ElementsList[el.index] = value ?? throw new NullReferenceException( "Cannot set null ResourceElement" );
            }
        }

        public ResourceElement? this[string key, string? context]
        {
            get => ElementsList.SingleOrDefault( el => el.Key == key && el.Context == context );
            set 
            {
                var el = ElementsList
                    .Select( ( element, index ) => ( element, index ) )
                    .SingleOrDefault( el => el.element.Key == key && el.element.Context == context );
                ElementsList[el.index] = value ?? throw new NullReferenceException( "Cannot set null ResourceElement" );
            }
        }

        public IEnumerator<ResourceElement> GetEnumerator()
        {
            return ElementsList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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
                ValuePlurals = plurals?.ToDictionary( p => p.Item1, p => p.Item2 ) ?? []
            };

            ElementsList.Add( el );
        }

        public void Remove( string key )
        {
            var index = ElementsList.FindIndex( el => el.Key == key );
            if ( index == -1 )
                throw new KeyNotFoundException( $"Element with \"{key}\" key not found" );
            ElementsList.RemoveAt( index );
        }
        public bool TryRemove( string key, out ResourceElement? element )
        {
            element = ElementsList.FirstOrDefault( el => el.Key == key );
            if ( element == null ) return false;

            ElementsList.Remove( element );
            return true;
        }
    }
}