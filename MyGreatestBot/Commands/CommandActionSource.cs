namespace MyGreatestBot.Commands
{
    /// <summary>
    /// Commands additional arguments
    /// </summary>
    [System.Flags]
    internal enum CommandActionSource : uint
    {
        None = 0x00000000U,
        Command = 0x00000001U,
        External = 0x00000002U,

        Mute = 0x10000000U
    }
}
