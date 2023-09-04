namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// API and services flags
    /// </summary>
    [System.Flags]
    public enum ApiIntents : int
    {
        None = 0x00,
        Youtube = 0x01,
        Yandex = 0x02,
        Vk = 0x04,
        Spotify = 0x08,
        Sql = 0x10,

        Music = Youtube | Yandex | Vk | Spotify,
        Services = Sql,

        Discord = None,

        All = Music | Services
    }
}
