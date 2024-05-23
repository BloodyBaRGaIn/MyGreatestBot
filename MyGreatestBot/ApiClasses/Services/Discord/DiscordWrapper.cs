global using DiscordWrapper = MyGreatestBot.ApiClasses.Services.Discord.DiscordWrapper;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;

namespace MyGreatestBot.ApiClasses.Services.Discord
{
    /// <summary>
    /// Discord API wrapper class
    /// </summary>
    public static class DiscordWrapper
    {
        public static readonly LogHandler CurrentDomainLogHandler = new(
            writer: Console.Out,
            guildName: AppDomain.CurrentDomain.FriendlyName,
            logDelay: 1000,
            defaultLogLevel: LogLevel.Information);

        public static readonly LogHandler CurrentDomainLogErrorHandler = new(
            writer: Console.Error,
            guildName: AppDomain.CurrentDomain.FriendlyName,
            logDelay: 1000,
            defaultLogLevel: LogLevel.Error);

        public static readonly DiscordBot Instance = new();

        /// <inheritdoc cref="DiscordBot.Client"/>
        [AllowNull] public static DiscordClient Client => Instance.Client;

        /// <inheritdoc cref="DiscordBot.Voice"/>
        [AllowNull] public static VoiceNextExtension VoiceNext => Instance.Voice;

        /// <inheritdoc cref="DiscordBot.Commands"/>
        [AllowNull] public static CommandsNextExtension Commands => Instance.Commands;

        /// <inheritdoc cref="CommandsNextExtension.RegisteredCommands"/>
        [AllowNull] public static IReadOnlyDictionary<string, Command> RegisteredCommands => Commands.RegisteredCommands;

        /// <summary>
        /// Try to run bot
        /// </summary>
        /// <param name="connectionTimeout">
        /// <inheritdoc cref="DiscordBot.Run(int, int)" path="/param[@name='connectionTimeout']"/>
        /// </param>
        /// <param name="disconnectionTimeout">
        /// <inheritdoc cref="DiscordBot.Run(int, int)" path="/param[@name='disconnectionTimeout']"/>
        /// </param>
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

        /// <inheritdoc cref="IAPI.Logout"/>
        public static void Logout()
        {
            (Instance as IAPI)?.Logout();
        }

        /// <inheritdoc cref="DiscordBot.Exit"/>
        public static void Exit()
        {
            Instance?.Exit();
        }
    }
}
