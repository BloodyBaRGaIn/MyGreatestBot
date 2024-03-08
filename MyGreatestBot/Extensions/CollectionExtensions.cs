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
            ArgumentNullException.ThrowIfNull(originCollection);

            Random random = new();

            foreach (T? value in originCollection.OrderBy(x => random.Next()))
            {
                yield return value;
            }
        }

        public static void EnqueueRangeToHead<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            List<T> head = [.. items];
            while (queue.Count != 0)
            {
                head.Add(queue.Dequeue());
            }

            queue.EnqueueRange(head);
        }

        public static void EnqueueToHead<T>(this Queue<T> queue, T item)
        {
            queue.EnqueueRangeToHead([item]);
        }

        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                queue.Enqueue(item);
            }
        }
    }
}
