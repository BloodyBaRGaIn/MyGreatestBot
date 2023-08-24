namespace DicordNET.ApiClasses
{
    /// <summary>
    /// API and services flags
    /// </summary>
    [System.Flags]
    internal enum ApiIntents : int
    {
        None = 0x00,
        Youtube = 0x01,
        Yandex = 0x02,
        Vk = 0x04,
        Spotify = 0x08,
        Sql = 0x10,

        All = Youtube | Yandex | Vk | Spotify | Sql
    }
}
