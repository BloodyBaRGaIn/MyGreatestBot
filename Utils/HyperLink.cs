namespace DicordNET.Utils
{
    internal sealed class HyperLink : IComparable<HyperLink>, IComparable
    {
        internal string Title { get; init; }
        private string Url { get; init; }

        internal HyperLink(string title, string? url = null)
        {
            Title = title;
            Url = url ?? string.Empty;
        }

        private string UpperTitle => Title.ToUpperInvariant();

        public static bool operator ==(HyperLink? left, HyperLink? right)
        {
            if (string.IsNullOrEmpty(left?.UpperTitle) || string.IsNullOrEmpty(right?.UpperTitle))
                return false;

            return left.UpperTitle == right.UpperTitle;
        }

        public static bool operator !=(HyperLink? left, HyperLink? right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is HyperLink link && this == link;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                return Title;
            }

            return $"[{Title}]({Url})";
        }

        int IComparable<HyperLink>.CompareTo(HyperLink? other)
        {
            return (this == other) ? 0 : 1;
        }

        int IComparable.CompareTo(object? obj)
        {
            if (obj is not HyperLink link)
            {
                return 1;
            }

            return (this == link) ? 0 : 1;
        }
    }
}
