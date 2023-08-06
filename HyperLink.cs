namespace DicordNET
{
    internal sealed class HyperLink
    {
        internal readonly string Title;
        internal readonly string Url;

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
