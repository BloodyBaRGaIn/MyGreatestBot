using Newtonsoft.Json;

namespace DicordNET.Config
{
    internal struct SpotifyClientSecretsJSON
    {
        [JsonProperty("clientId")]
        public string ClientId { get; private set; }
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; private set; }
    }
}
