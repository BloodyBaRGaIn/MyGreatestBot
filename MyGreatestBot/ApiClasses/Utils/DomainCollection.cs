using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.ApiClasses.Utils
{
    public sealed class DomainCollection : IEnumerable<string>, IEnumerable
    {
        private readonly List<string> collection;

        public DomainCollection(params string[] domains)
        {
            if (domains == null)
            {
                throw new ArgumentNullException(nameof(domains), "Input collection is null");
            }
            if (domains.Length == 0)
            {
                throw new ArgumentException("Input collection is empty", nameof(domains));
            }
            collection = [.. domains];
        }

        private string GetPrimary()
        {
            return collection.Where(c => !string.IsNullOrWhiteSpace(c)).FirstOrDefault() ?? string.Empty;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        public override string ToString()
        {
            return GetPrimary();
        }

        public static implicit operator string(DomainCollection domainCollection)
        {
            return domainCollection.GetPrimary();
        }

        public static implicit operator DomainCollection(string domain)
        {
            return new(domain);
        }
    }
}
