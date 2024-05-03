using Newtonsoft.Json;

namespace MyGreatestBot.ApiClasses.ConfigStructs
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
