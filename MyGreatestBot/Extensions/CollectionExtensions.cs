using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.Extensions
{
    /// <summary>
    /// <see cref="IEnumerable{T}"/> extensions
    /// </summary>
    internal static class CollectionExtensions
    {
        internal static IEnumerable<T> Shuffle<T>(this IEnumerable<T> origin_collection)
        {
            if (origin_collection == null)
            {
                throw new ArgumentNullException(nameof(origin_collection));
            }

            if (origin_collection.Count() < 2)
            {
                return origin_collection;
            }

            Random random = new();
            List<T> result = new();
            result.AddRange(origin_collection);
            return result.OrderBy(x => random.Next());
        }
    }
}
