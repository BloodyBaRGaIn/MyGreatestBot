namespace MyGreatestBot.ApiClasses.Utils
{
    /// <summary>
    /// Hypertext string
    /// </summary>
    /// <param name="title">Text</param>
    /// <param name="url">URL</param>
    public sealed class HyperLink(string title, string? url = null)
    {
        /// <summary>
        /// Displayed text
        /// </summary>
        public string Title { get; } = string.IsNullOrWhiteSpace(title) ? "Blank title" : title;

        /// <summary>
        /// URL
        /// </summary>
        public string Url { get; } = url ?? string.Empty;

        public CompositeId InnerId { get; private set; } = new();

        public HyperLink WithId(CompositeId id)
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
            // broken names
            string temp_title = Title.Replace("://", " ");

            return string.IsNullOrWhiteSpace(Url) ? temp_title : $"[{temp_title}]({Url})";
        }
    }
}
