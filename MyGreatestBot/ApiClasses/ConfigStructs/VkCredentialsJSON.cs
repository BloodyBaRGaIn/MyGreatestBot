using Newtonsoft.Json;

namespace MyGreatestBot.ApiClasses.ConfigStructs
{
    /// <summary>
    /// Vk credentials content
    /// </summary>
    internal struct VkCredentialsJSON
    {
        [JsonProperty("appid")]
        public string AppId { get; private set; }
        [JsonProperty("username")]
        public string Username { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }
    }
}
