using System.Collections.Generic;
using System.Linq;

namespace nresx.Tools.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool TryTake<T>( this List<T> source, out T item )
        {
            if ( source?.Count > 0 )
            {
                var first = source[0];
                source.RemoveAt( 0 );
                item = first;
                return true;
            }

            item = default;
            return false;
        }

        public static bool TryTakeRange<T>( this List<T> source, out List<T> items, int maxCount = -1 )
        {
            items = new List<T>();
            while ( ( maxCount == -1 || items.Count <= maxCount ) && source.TryTake( out var item ) )
            {
                items.Add( item );
            }

            return items.Any();
        }

        public static T Take<T>( this List<T> source )
        {
            if ( TryTake( source, out var item ) )
                return item;
            return default;
        }
    }
}
