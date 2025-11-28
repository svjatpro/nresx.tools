using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace nresx.Tools
{
    public sealed class ResourceElements( IEnumerable<ResourceElement>? elements = null )
        : IEnumerable<ResourceElement>
    {
        private readonly List<ResourceElement> ElementsList = elements?.ToList() ?? [];

        public ResourceElement? this[ int index ] => ElementsList[index];

        //set => ElementsList[index] = value;
        public ResourceElement? this[string key] => ElementsList.SingleOrDefault( el => el.Key == key );

        //set => ElementsList[key] = value;
        public IEnumerator<ResourceElement> GetEnumerator()
        {
            return ElementsList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add( string key, string value, string? comment = null )
        {
            var el = new ResourceElement
            {
                Type = ResourceElementType.String,
                Key = key,
                Value = value,
                Comment = comment
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