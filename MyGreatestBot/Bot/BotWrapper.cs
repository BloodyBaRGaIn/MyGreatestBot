using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using System;
using System.Runtime.Versioning;

namespace DicordNET.Bot
{
    [SupportedOSPlatform("windows")]
    internal static class BotWrapper
    {
        internal static readonly DiscordBot BotInstance = new();

        internal static DiscordClient? Client => BotInstance.Client;
        internal static VoiceNextExtension? VoiceNext => BotInstance.Voice;
        internal static CommandsNextExtension? Commands => BotInstance.Commands;
        internal static IServiceProvider ServiceProvider => BotInstance.ServiceProvider;

        internal static void Run()
        {
            BotInstance.RunAsync().GetAwaiter().GetResult();
        }
    }
}
