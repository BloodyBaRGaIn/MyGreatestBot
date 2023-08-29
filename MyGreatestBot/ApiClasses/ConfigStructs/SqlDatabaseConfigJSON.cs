using Newtonsoft.Json;

namespace MyGreatestBot.ApiClasses.ConfigStructs
{
    /// <summary>
    /// Sql database config content
    /// </summary>
    internal struct SqlDatabaseConfigJSON
    {
        [JsonProperty("local_dir")]
        public string LocalDirectory { get; private set; }
        [JsonProperty("local_name")]
        public string DatabaseName { get; private set; }
        [JsonProperty("server_name")]
        public string ServerName { get; private set; }
    }
}
