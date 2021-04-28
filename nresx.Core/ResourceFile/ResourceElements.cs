using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace nresx.Tools
{
    public sealed class ResourceElements : IEnumerable<ResourceElement>
    {
        private readonly List<ResourceElement> ElementsList;

        public ResourceElement this[ int key ]
        {
            get => ElementsList[key];
            set => ElementsList[key] = value;
        }

        public IEnumerator<ResourceElement> GetEnumerator()
        {
            return ElementsList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ResourceElements( IEnumerable<ResourceElement> elements = null )
        {
            ElementsList = elements?.ToList() ?? new List<ResourceElement>();
        }

        public void Add( string key, string value, string comment = null )
        {
            ElementsList.Add( new ResourceElement
            {
                Type = ResourceElementType.String,
                Key = key,
                Value = value,
                Comment = comment
            } );
        }

        public void Remove( string key )
        {
            var index = ElementsList.FindIndex( el => el.Key == key );
            if ( index == -1 )
                throw new KeyNotFoundException( $"Element with \"{key}\" key not found" );
            ElementsList.RemoveAt( index );
        }
        public bool TryRemove( string key, out ResourceElement element )
        {
            element = ElementsList.FirstOrDefault( el => el.Key == key );
            if ( element != null )
            {
                ElementsList.Remove( element );
                return true;
            }

            return false;
        }
    }
}