using System;

namespace MyGreatestBot.Utils
{
    /// <summary>
    /// Hypertext string
    /// </summary>
    public sealed class HyperLink : IComparable<HyperLink>
    {
        /// <summary>
        /// Displayed text
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// URL
        /// </summary>
        public string Url { get; init; }

        public string InnerId { get; private set; } = string.Empty;

        /// <summary>
        /// Generic constructor
        /// </summary>
        /// <param name="title">Text</param>
        /// <param name="url">URL</param>
        public HyperLink(string title, string? url = null)
        {
            Title = title;
            Url = url ?? string.Empty;
        }

        public HyperLink WithId(string id)
        {
            InnerId = id;
            return this;
        }

        /// <summary>
        /// Overriden <see cref="object.ToString"/>
        /// </summary>
        /// <returns>Hypertext string</returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                return Title;
            }

            return $"[{Title}]({Url})";
        }

        /// <summary>
        /// Comparsion method
        /// </summary>
        /// <param name="other">Other instance</param>
        /// <returns>Zero if equals</returns>
        public int CompareTo(HyperLink? other)
        {
            if (this is null || other is null)
            {
                return int.MaxValue;
            }
            return other.Title.CompareTo(Title);
        }
    }
}
