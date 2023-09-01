using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.Extensions
{
    /// <summary>
    /// <see cref="IEnumerable{T}"/> extensions
    /// </summary>
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> originCollection)
        {
            if (originCollection == null)
            {
                throw new ArgumentNullException(nameof(originCollection));
            }

            Random random = new();

            foreach (var value in originCollection.OrderBy(x => random.Next()))
            {
                yield return value;
            }
        }
    }
}
