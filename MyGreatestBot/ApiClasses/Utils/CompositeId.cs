namespace MyGreatestBot.ApiClasses.Utils
{
    /// <summary>
    /// Represents a stringified identifier with corresponded API
    /// </summary>
    /// <param name="id">ID string</param>
    /// <param name="intents">API flag</param>
    public sealed class CompositeId(string id = "", ApiIntents intents = ApiIntents.None)
    {
        /// <summary>
        /// ID string
        /// </summary>
        public string Id { get; } = id;

        /// <summary>
        /// API flag
        /// </summary>
        public ApiIntents Api { get; } = intents;

        public override string ToString()
        {
            return Id;
        }

        public static implicit operator string(CompositeId instance)
        {
            return instance.ToString();
        }
    }
}
