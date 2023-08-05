using Newtonsoft.Json;

namespace DicordNET.Config
{
    internal struct GoogleCredentialsJSON
    {
        [JsonProperty("username")]
        public string Username { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }
        [JsonProperty("key")]
        public string Key { get; private set; }
    }
}
