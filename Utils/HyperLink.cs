namespace DicordNET.Utils
{
    internal sealed class HyperLink
    {
        internal string Title { get; init; }
        private string Url { get; init; }

        internal HyperLink(string title, string? url = null)
        {
            Title = title;
            Url = url ?? string.Empty;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                return Title;
            }

            return $"[{Title}]({Url})";
        }
    }
}
