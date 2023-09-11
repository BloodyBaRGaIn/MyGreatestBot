using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Services.Discord
{
    public static class DiscordWrapper
    {
        public static readonly DiscordBot Instance = new();
        public static IServiceProvider ServiceProvider => Instance.ServiceProvider;

        [AllowNull]
        public static DiscordClient Client => Instance.Client;
        [AllowNull]
        public static VoiceNextExtension VoiceNext => Instance.Voice;
        [AllowNull]
        public static CommandsNextExtension Commands => Instance.Commands;

        public static void Run(int connection_timeout)
        {
            Instance.RunAsync(connection_timeout).GetAwaiter().GetResult();
        }
    }
}
