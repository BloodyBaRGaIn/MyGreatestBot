using Newtonsoft.Json;

namespace MyGreatestBot.ApiClasses.ConfigClasses.JsonModels
{
    /// <summary>
    /// Spotify client secret content
    /// </summary>
    internal struct SpotifyCredentialsJSON
    {
        [JsonProperty("clientId")]
        public string ClientId { get; private set; }
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; private set; }
        [JsonProperty("token")]
        public string Token { get; private set; }
    }
}
