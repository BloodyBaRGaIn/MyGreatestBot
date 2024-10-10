using Newtonsoft.Json;

namespace MyGreatestBot.ApiClasses.ConfigClasses.JsonModels
{
    /// <summary>
    /// Yandex credentials content
    /// </summary>
    internal struct YandexCredentialsJSON
    {
        [JsonProperty("username")]
        public string Username { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }
    }
}
