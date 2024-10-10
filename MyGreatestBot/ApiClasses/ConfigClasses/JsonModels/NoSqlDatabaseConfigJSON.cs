using Newtonsoft.Json;

namespace MyGreatestBot.ApiClasses.ConfigClasses.JsonModels
{
    /// <summary>
    /// NoSql database config content
    /// </summary>
    internal struct NoSqlDatabaseConfigJSON
    {
        [JsonProperty("local_name")]
        public string DatabaseName { get; private set; }
    }
}
