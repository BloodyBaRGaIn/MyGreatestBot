using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using MyGreatestBot.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace MyGreatestBot.ApiClasses.Services.Discord
{
    public static class DiscordWrapper
    {
        public static readonly Handlers.LogHandler CurrentDomainLogHandler
            = new(Console.Out, AppDomain.CurrentDomain.FriendlyName, LogLevel.Information);

        public static readonly Handlers.LogHandler CurrentDomainLogErrorHandler
            = new(Console.Error, AppDomain.CurrentDomain.FriendlyName, LogLevel.Error);

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
            try
            {
                Instance.RunAsync(connection_timeout).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                CurrentDomainLogErrorHandler.Send(ex.GetExtendedMessage());
            }
        }
    }
}
