using System.Collections.Generic;
using System.Linq;

namespace LD48
{
    public static class EnumerableExtensions
    {
        // TODO: Move into the SDK
        public static bool None<TSource>(this IEnumerable<TSource> enumerable)
        {
            return !enumerable.Any();
        }
    }
}