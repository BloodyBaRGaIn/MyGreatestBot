using Newtonsoft.Json;

namespace DicordNET.Config
{
    internal struct YandexCredentialsJSON
    {
        [JsonProperty("username")]
        public string Username { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }
    }
}
