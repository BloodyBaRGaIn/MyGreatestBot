namespace DicordNET.Commands
{
    [Flags]
    internal enum CommandActionSource : uint
    {
        None = 0x00,
        Command = 0x01,
        External = 0x02,


        Mute = 0x10000000
    }
}
