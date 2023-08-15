﻿using Newtonsoft.Json;

namespace DicordNET.Config
{
    /// <summary>
    /// Spotify client secret content
    /// </summary>
    internal struct SpotifyClientSecretsJSON
    {
        [JsonProperty("clientId")]
        public string ClientId { get; private set; }
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; private set; }
        [JsonProperty("token")]
        public string Token { get; private set; }
    }
}
