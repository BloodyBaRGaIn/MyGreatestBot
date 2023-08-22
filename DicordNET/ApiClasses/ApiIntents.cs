namespace DicordNET.ApiClasses
{
    /// <summary>
    /// API flags
    /// </summary>
    [Flags]
    internal enum ApiIntents : uint
    {
        None = 0x00,
        Youtube = 0x01,
        Yandex = 0x02,
        Vk = 0x04,
        Spotify = 0x08,

        All = Youtube | Yandex | Vk | Spotify
    }
}
