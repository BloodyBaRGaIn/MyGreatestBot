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
        public const int DefaultConnectionTimeout = 10000;
        public const int DefaultDisconnectionTimeout = 500;

        public static int ConnectionTimeout { get; private set; } = DefaultConnectionTimeout;
        public static int DisconnectionTimeout { get; private set; } = DefaultDisconnectionTimeout;

        public static LogHandler CurrentDomainLogHandler { get; } = new(
            writer: Console.Out,
            guildName: AppDomain.CurrentDomain.FriendlyName,
            logDelay: 1000,
            defaultLogLevel: LogLevel.Information);

        public static LogHandler CurrentDomainLogErrorHandler { get; } = new(
            writer: Console.Error,
            guildName: AppDomain.CurrentDomain.FriendlyName,
            logDelay: 1000,
            defaultLogLevel: LogLevel.Error);

        public static DiscordBot Instance { get; } = new();

        /// <inheritdoc cref="DiscordBot.Client"/>
        [AllowNull] public static DiscordClient Client => Instance.Client;

        /// <inheritdoc cref="DiscordBot.Voice"/>
        [AllowNull] public static VoiceNextExtension VoiceNext => Instance.Voice;

        /// <inheritdoc cref="DiscordBot.Commands"/>
        [AllowNull] public static CommandsNextExtension Commands => Instance.Commands;

        /// <inheritdoc cref="CommandsNextExtension.RegisteredCommands"/>
        [AllowNull] public static IReadOnlyDictionary<string, Command> RegisteredCommands => Commands.RegisteredCommands;

        /// <inheritdoc cref="DiscordBot.Age"/>
        public static int Age => Instance.Age;

        /// <summary>
        /// Try to run bot with default timeouts.
        /// </summary>
        public static void Run()
        {
            Run(DefaultConnectionTimeout, DefaultDisconnectionTimeout);
        }

        /// <summary>
        /// Try to run bot.
        /// </summary>
        /// <param name="connectionTimeout">
        /// Connection timeout in milliseconds.
        /// </param>
        /// <param name="disconnectionTimeout">
        /// Disconnection timeout in milliseconds.
        /// </param>
        public static void Run(int connectionTimeout, int disconnectionTimeout)
        {
            ConnectionTimeout = connectionTimeout > 0 ? connectionTimeout : DefaultConnectionTimeout;
            DisconnectionTimeout = disconnectionTimeout > 0 ? disconnectionTimeout : DefaultDisconnectionTimeout;

            try
            {
                Instance?.Run();
            }
            catch (Exception ex)
            {
                CurrentDomainLogErrorHandler.Send(ex.GetExtendedMessage());
            }
            finally
            {
                CurrentDomainLogHandler.Dispose();
                CurrentDomainLogErrorHandler.Dispose();
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
