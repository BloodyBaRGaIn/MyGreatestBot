using Newtonsoft.Json;

namespace MyGreatestBot.ApiClasses.ConfigStructs
{
    /// <summary>
    /// Discord config content
    /// </summary>
    internal struct DiscordConfigJSON
    {
        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
        [JsonProperty("token")]
        public string Token { get; private set; }
    }
}
