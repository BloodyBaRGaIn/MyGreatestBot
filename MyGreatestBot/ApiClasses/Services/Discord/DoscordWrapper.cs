using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

namespace MyGreatestBot.ApiClasses.Services.Discord
{
    [SupportedOSPlatform("windows")]
    public static class DoscordWrapper
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
