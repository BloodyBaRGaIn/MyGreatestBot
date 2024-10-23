namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// API and services flags
    /// </summary>
    [System.Flags]
    public enum ApiIntents : uint
    {
        None = 0x00000000U,
        Youtube = 0x00000001U,
        Yandex = 0x00000002U,
        Vk = 0x00000004U,
        Spotify = 0x00000008U,

        NoSql = 0x20000000U,
        Sql = 0x40000000U,

        Db = NoSql,

        Discord = 0x80000000U,

        Services = Db | Discord,

        Music = Youtube | Yandex | Vk | Spotify,

        All = Music | Services
    }
}
