using System;
using System.Collections;
using System.Collections.Generic;

namespace MyGreatestBot.Utils
{
    public sealed class DomainCollection : IEnumerable<string>
    {
        private readonly List<string> collection;

        public DomainCollection(params string[] domains)
        {
            if (domains == null)
            {
                throw new ArgumentNullException(nameof(domains));
            }
            if (domains.Length == 0)
            {
                throw new ArgumentException(nameof(domains));
            }
            collection = new(domains);
        }

        private string GetPrimary()
        {
            return collection[0];
        }

        public IEnumerator<string> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        public static implicit operator string(DomainCollection domainCollection)
        {
            return domainCollection.GetPrimary();
        }
    }
}
