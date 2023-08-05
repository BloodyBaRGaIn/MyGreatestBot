using Newtonsoft.Json;

namespace DicordNET.Config
{
    internal struct DiscordConfigJSON
    {
        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
        [JsonProperty("token")]
        public string Token { get; private set; }
    }
}
