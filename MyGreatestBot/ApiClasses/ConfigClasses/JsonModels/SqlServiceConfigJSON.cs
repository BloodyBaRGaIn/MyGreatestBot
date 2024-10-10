using Newtonsoft.Json;

namespace MyGreatestBot.ApiClasses.ConfigClasses.JsonModels
{
    /// <summary>
    /// Sql service config content
    /// </summary>
    internal struct SqlServiceConfigJSON
    {
        [JsonProperty("service_browser")]
        public string BrowserServiceName { get; private set; }
        [JsonProperty("service_server")]
        public string ServerServiceName { get; private set; }
        [JsonProperty("service_server_arg")]
        public string ServerServiceArgument { get; private set; }
    }
}
