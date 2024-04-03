using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;

using AllowNullAttribute = System.Diagnostics.CodeAnalysis.AllowNullAttribute;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace MyGreatestBot.ApiClasses.Services.Discord
{
    public static class DiscordWrapper
    {
        public static readonly LogHandler CurrentDomainLogHandler
            = new(Console.Out, AppDomain.CurrentDomain.FriendlyName, LogLevel.Information);

        public static readonly LogHandler CurrentDomainLogErrorHandler
            = new(Console.Error, AppDomain.CurrentDomain.FriendlyName, LogLevel.Error);

        public static readonly DiscordBot Instance = new();

        [AllowNull]
        public static DiscordClient Client => Instance.Client;
        [AllowNull]
        public static VoiceNextExtension VoiceNext => Instance.Voice;
        [AllowNull]
        public static CommandsNextExtension Commands => Instance.Commands;
        [AllowNull]
        public static IReadOnlyDictionary<string, Command> RegisteredCommands => Commands.RegisteredCommands;

        /// <summary>
        /// Try to run bot
        /// </summary>
        /// <param name="connectionTimeout">Timeout for connection</param>
        /// <param name="disconnectionTimeout">Timeout for disconnection</param>
        public static void Run(int connectionTimeout, int disconnectionTimeout)
        {
            try
            {
                Instance?.Run(connectionTimeout, disconnectionTimeout);
            }
            catch (Exception ex)
            {
                CurrentDomainLogErrorHandler.Send(ex.GetExtendedMessage());
            }
        }

        /// <summary>
        /// Bot stop request
        /// </summary>
        public static void Exit()
        {
            Instance?.Exit();
        }
    }
}
