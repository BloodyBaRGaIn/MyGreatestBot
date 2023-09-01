using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

namespace MyGreatestBot.Bot
{
    [SupportedOSPlatform("windows")]
    public static class BotWrapper
    {
        private static readonly DiscordBot BotInstance = new();
        public static IServiceProvider ServiceProvider => BotInstance.ServiceProvider;

        [AllowNull]
        public static DiscordClient Client => BotInstance.Client;
        [AllowNull]
        public static VoiceNextExtension VoiceNext => BotInstance.Voice;
        [AllowNull]
        public static CommandsNextExtension Commands => BotInstance.Commands;
        [AllowNull]
        public static Exception LastError { get; set; }

        public static void Run(int connection_timeout)
        {
            BotInstance.RunAsync(connection_timeout).GetAwaiter().GetResult();
        }
    }
}
