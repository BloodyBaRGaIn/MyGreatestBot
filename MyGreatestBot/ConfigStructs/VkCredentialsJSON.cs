using Newtonsoft.Json;

namespace MyGreatestBot.ConfigStructs
{
    /// <summary>
    /// Vk credentials content
    /// </summary>
    internal struct VkCredentialsJSON
    {
        [JsonProperty("username")]
        public string Username { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }
    }
}
