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

        Sql = 0x40000000U,
        Discord = 0x80000000U,

        Music = Youtube | Yandex | Vk | Spotify,
        Services = Sql | Discord,

        All = Music | Services
    }
}
