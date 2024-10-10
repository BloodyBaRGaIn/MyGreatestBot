using Newtonsoft.Json;

namespace MyGreatestBot.ApiClasses.ConfigClasses.JsonModels
{
    /// <summary>
    /// Google credentials content
    /// </summary>
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
