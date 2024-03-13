﻿namespace MyGreatestBot.Commands.Utils
{
    /// <summary>
    /// Commands additional arguments
    /// </summary>
    [System.Flags]
    internal enum CommandActionSource : uint
    {
        None = 0x00000000U,
        Command = 0x00000001U,
        Event = 0x00000002U,

        PlayerToHead = 0x00000100U,
        PlayerShuffle = 0x00000200U,
        PlayerSkipCurrent = 0x00000400U,
        PlayerRadio = 0x00000800U,
        PlayerNoBlacklist = 0x00001000U,

        Mute = 0x10000000U
    }
}
