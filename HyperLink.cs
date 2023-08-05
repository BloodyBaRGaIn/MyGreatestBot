namespace DicordNET
{
    internal sealed class HyperLink
    {
        internal readonly string Title;
        internal readonly string Url;

        internal HyperLink(string title, string url)
        {
            Title = title;
            Url = url;
        }

        public override string ToString()
        {
            return $"[{Title}]({Url})";
        }
    }
}
