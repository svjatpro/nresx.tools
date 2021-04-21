using System.Collections.Generic;

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
    }
}
