using System.Collections.Generic;
using System.Linq;
using Utilities.Monads;

namespace LD48
{
    public static class EnumerableExtensions
    {
        // TODO: Move into the SDK
        public static bool None<TSource>(this IEnumerable<TSource> enumerable)
        {
            return !enumerable.Any();
        }
        // TODO: Move into the SDK
        public static IMaybe<T> GetElementByIndexOrEmpty<T>(this IEnumerable<T> enumerable, int index)
        {
            return enumerable.Count() > index ? Maybe.Of(enumerable.ElementAt(index)) : Maybe.Empty<T>();
        }
    }
}