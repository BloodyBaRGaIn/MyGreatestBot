using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Commands;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord
{
    public sealed class DiscordBot : IAPI, IAccessible
    {
        /// <summary>
        /// Bot client instance.
        /// </summary>
        [AllowNull] public DiscordClient Client { get; private set; }

        /// <summary>
        /// Bot interactivity instance.
        /// </summary>
        [AllowNull] public InteractivityExtension Interactivity { get; private set; }

        /// <summary>
        /// Bot commands handling instance.
        /// </summary>
        [AllowNull] public CommandsNextExtension Commands { get; private set; }

        /// <summary>
        /// Bot voice actions handling instance.
        /// </summary>
        [AllowNull] public VoiceNextExtension Voice { get; private set; }

        /// <summary>
        /// Bot's age. Zero if it's not its anniversary today.
        /// </summary>
        public int Age { get; private set; } = -1;

        private ServiceProvider ServiceProvider { get; } = new ServiceCollection().BuildServiceProvider();

        /// <summary>
        /// Default bot's command prefix
        /// </summary>
        private const string DefaultPrefix = "d!";

        /// <summary>
        /// Actual bot's command prefix
        /// </summary>
        private string CommandPrefix = string.Empty;

        private volatile bool exitRequest;

        ApiIntents IAPI.ApiType => ApiIntents.Discord;

        bool IAPI.IsEssential => true;

        DomainCollection IAccessible.Domains { get; } = "http://www.discord.com/";

        void IAPI.PerformAuth()
        {
            DiscordConfigJSON config_js = ConfigManager.GetDiscordConfigJSON();

            DiscordConfiguration discordConfig = new()
            {
                MinimumLogLevel = LogLevel.Error,
                LogTimestampFormat = LogHandler.DateTimeFormat,
                Intents = DiscordIntents.All,
                Token = config_js.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            CommandPrefix = config_js.Prefix;

            Client = new(discordConfig);

            Client.SessionCreated += Client_SessionCreated;
            Client.ClientErrored += Client_ClientErrored;
            Client.SocketErrored += Client_SocketErrored;
            Client.SocketClosed += Client_SocketClosed;
            Client.VoiceStateUpdated += Client_VoiceStateUpdated;
            Client.VoiceServerUpdated += Client_VoiceServerUpdated;

            Interactivity = Client.UseInteractivity(new()
            {
                Timeout = TimeSpan.FromMinutes(20)
            });

            if (string.IsNullOrWhiteSpace(CommandPrefix))
            {
                CommandPrefix = DefaultPrefix;
                DiscordWrapper.CurrentDomainLogErrorHandler.Send("Command prefix set to its default value", LogLevel.Warning);
            }

            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = [CommandPrefix],
                CaseSensitive = false,
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
                Services = ServiceProvider,
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.SetHelpFormatter<CustomHelpFormatter>();
            Commands.RegisterCommands<ConnectionCommands>();
            Commands.RegisterCommands<QueuingCommands>();
            Commands.RegisterCommands<PlaybackCommands>();
            Commands.RegisterCommands<DatabaseCommands>();
            Commands.RegisterCommands<DebugCommands>();

            Commands.CommandErrored += Commands_CommandErrored;
            Commands.CommandExecuted += Commands_CommandExecuted;

            MarkdownWriter.GenerateFile();
        }

        void IAPI.Logout()
        {
            if (Commands != null)
            {
                if (Commands.RegisteredCommands.Count != 0)
                {
                    try
                    {
                        Commands.UnregisterCommands(cmds: Commands.RegisteredCommands.Values.ToArray());
                    }
                    catch { }
                }

                Commands.CommandExecuted -= Commands_CommandExecuted;
                Commands.CommandErrored -= Commands_CommandErrored;
            }

            if (Client != null)
            {
                Client.VoiceStateUpdated -= Client_VoiceStateUpdated;
                Client.VoiceServerUpdated -= Client_VoiceServerUpdated;
                Client.SessionCreated -= Client_SessionCreated;
                Client.ClientErrored -= Client_ClientErrored;
                Client.SocketErrored -= Client_SocketErrored;
                Client.SocketClosed -= Client_SocketClosed;
                Client.VoiceStateUpdated -= Client_VoiceStateUpdated;
                Client.VoiceServerUpdated -= Client_VoiceServerUpdated;
            }
        }

        /// <summary>
        /// Runs bot
        /// </summary>
        /// <param name="connectionTimeout">Timeout for connection</param>
        /// <param name="disconnectionTimeout">Timeout for disconnection</param>
        internal void Run(int connectionTimeout, int disconnectionTimeout)
        {
            // try to start
            try
            {
                if (Client == null)
                {
                    throw new DiscordApiException();
                }

                if (!Client.ConnectAsync().Wait(connectionTimeout))
                {
                    throw new DiscordApiException("Cannot connect to Discord");
                }
                Voice = Client.UseVoiceNext();
            }
            catch (Exception ex)
            {
                Disconnect(disconnectionTimeout);

                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    string.Join(Environment.NewLine,
                        ex.GetExtendedMessage(),
                        "Press any key to exit"));

                _ = Console.ReadKey(true);
                return;
            }

            // waiting for stop request
            while (true)
            {
                if (exitRequest)
                {
                    break;
                }
                try
                {
                    Thread.Sleep(1);
                }
                catch
                {
                    break;
                }
            }

            if (Client == null)
            {
                return;
            }

            // try to set offline status
            try
            {
                _ = Client.UpdateStatusAsync(
                    new(), DiscordUserStatus.Offline)
                .Wait(disconnectionTimeout);
            }
            catch { }

            Disconnect(disconnectionTimeout);
        }

        /// <summary>
        /// Command with parameters to string
        /// </summary>
        /// <param name="args">Command</param>
        /// <returns></returns>
        private static string GetCommandInfo(CommandEventArgs args)
        {
            string result = string.Empty;
            if (args.Context.Member is not null)
            {
                result += $"{args.Context.Member.DisplayName} : ";
            }

            if (args.Command != null)
            {
                result += $"{args.Command.Name}";
            }

            if (!string.IsNullOrWhiteSpace(args.Context.RawArgumentString))
            {
                result += $" {args.Context.RawArgumentString}";
            }

            return result;
        }

        /// <summary>
        /// Bot stop request
        /// </summary>
        internal void Exit()
        {
            exitRequest = true;
        }

        /// <summary>
        /// Try to disconnect with timeout
        /// </summary>
        /// <param name="disconnectionTimeout">Timeout</param>
        private void Disconnect(int disconnectionTimeout)
        {
            if (Client == null)
            {
                return;
            }
            try
            {
                _ = Client.DisconnectAsync().Wait(disconnectionTimeout);
                Client.Dispose();
            }
            catch { }
        }

        #region Private event handlers

        private async Task Client_SessionCreated(DiscordClient sender, SessionReadyEventArgs args)
        {
            await sender.UpdateStatusAsync(new()
            {
                ActivityType = DiscordActivityType.ListeningTo,
                Name = $"{CommandPrefix}{CommandStrings.HelpCommandName}"
            }, DiscordUserStatus.Online);

            DiscordWrapper.CurrentDomainLogHandler.Send("Session created.");

            if (Age == -1)
            {
                DateTime birthdate =
                    Client.CurrentUser.CreationTimestamp.Date
                //new(DateTime.Today.Year - 5, DateTime.Today.Month, DateTime.Today.Day);
                ;

                DateTime today = DateTime.Today;

                int age = today.Year - birthdate.Year;

                if (birthdate.Date > today.AddYears(-age))
                {
                    age--;
                }

                if (age >= 0)
                {
                    Age = age;
                }

                if (age > 0 && birthdate.Month == today.Month && birthdate.Day == today.Day)
                {
                    DiscordWrapper.CurrentDomainLogHandler.Send(
                        $"It's my {Age} year anniversary today!!!");
                }
            }
        }

        private async Task Client_ClientErrored(DiscordClient sender, ClientErrorEventArgs args)
        {
            await DiscordWrapper.CurrentDomainLogErrorHandler.SendAsync(
                args.Exception.GetExtendedMessage());
        }

        private async Task Client_SocketErrored(DiscordClient sender, SocketErrorEventArgs args)
        {
            await DiscordWrapper.CurrentDomainLogErrorHandler.SendAsync(
                args.Exception.GetExtendedMessage());
        }

        private async Task Client_SocketClosed(DiscordClient sender, SocketCloseEventArgs args)
        {
            await DiscordWrapper.CurrentDomainLogErrorHandler.SendAsync(
                args.CloseMessage);
        }

        private async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            bool isBotVoiceStateUpdated = e.User.Id == client.CurrentUser.Id && e.User.IsBot;
            if (!isBotVoiceStateUpdated)
            {
                return;
            }

            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(e.Guild);
            if (handler == null)
            {
                return;
            }

            handler.VoiceUpdating = true;

#pragma warning disable CS8604
            bool channel_changed = (e.After?.Channel) != (e.Before?.Channel);
#pragma warning restore CS8604

            if (!channel_changed)
            {
                return;
            }

            bool semaphoreReady = handler.VoiceUpdateSemaphore.TryWaitOne(1000);
            if (semaphoreReady)
            {
                Client.VoiceStateUpdated -= Client_VoiceStateUpdated;
            }

            try
            {
                if (semaphoreReady)
                {
                    if (e.After?.Channel is not null)
                    {
                        await handler.Join(e);
                        await handler.Voice.WaitForConnectionAsync();
                    }
                    else
                    {
                        if (!handler.Voice.IsManualDisconnect)
                        {
                            await Task.Run(() => handler.PlayerInstance.Stop(CommandActionSource.Event | CommandActionSource.Mute));
                            handler.Message.Send(new DiscordEmbedBuilder()
                            {
                                Color = DiscordColor.Red,
                                Title = "Kicked from voice channel"
                            });
                            handler.Voice.Disconnect(false);
                        }
                    }
                }
                else
                {
                    handler.Message.Send(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Failed to update voice state"
                    });
                }
            }
            catch (Exception ex)
            {
                handler.Message.Send(ex);
            }

            handler.Update(e.Guild);

            if (semaphoreReady)
            {
                Client.VoiceStateUpdated += Client_VoiceStateUpdated;
                _ = handler.VoiceUpdateSemaphore.TryRelease();
            }

            handler.VoiceUpdating = false;
        }

        private async Task Client_VoiceServerUpdated(DiscordClient client, VoiceServerUpdateEventArgs e)
        {
            bool isBotVoiceServerUpdated = Client.CurrentUser.Id == client.CurrentUser.Id;
            if (!isBotVoiceServerUpdated)
            {
                return;
            }

            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(e.Guild);
            if (handler == null)
            {
                return;
            }

            while (handler.ServerUpdating)
            {
                await Task.Delay(1);
                await Task.Yield();
            }

            handler.ServerUpdating = true;

            bool semaphoreReady = handler.ServerUpdateSemaphore.TryWaitOne(1000);
            if (!semaphoreReady)
            {
                return;
            }

            Client.VoiceServerUpdated -= Client_VoiceServerUpdated;

            await handler.Reconnect();

            handler.Update(e.Guild);

            Client.VoiceServerUpdated += Client_VoiceServerUpdated;
            _ = handler.ServerUpdateSemaphore.TryRelease();

            handler.ServerUpdating = false;

            await Task.Delay(1);
        }

        private async Task Commands_CommandExecuted(
            CommandsNextExtension sender,
            CommandExecutionEventArgs args)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(args.Context.Guild);
            if (handler != null)
            {
                await handler.Log.SendAsync(GetCommandInfo(args));
            }
        }

        private async Task Commands_CommandErrored(
            CommandsNextExtension sender,
            CommandErrorEventArgs args)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(args.Context.Guild);
            if (handler == null)
            {
                return;
            }

            bool handled = false;

            switch (args.Exception)
            {
                case CommandNotFoundException:
                    // try search command without ending '\'
                    if (args.Command != null)
                    {
                        break;
                    }

                    string? badCommandText = args.Context.Message.Content;

                    if (badCommandText == null)
                    {
                        return;
                    }

                    if (badCommandText.StartsWith(CommandPrefix))
                    {
                        badCommandText = badCommandText[CommandPrefix.Length..];
                    }

                    int firstSpaceIndex = badCommandText.IndexOf(' ');
                    string? rawArguments = null;

                    if (firstSpaceIndex != -1)
                    {
                        rawArguments = badCommandText[firstSpaceIndex..].Trim();
                        badCommandText = badCommandText[..firstSpaceIndex];
                    }

                    badCommandText = badCommandText.TrimEnd('\\');

                    Command? findCommand = Commands.FindCommand(badCommandText, out _);

                    if (findCommand == null)
                    {
                        break;
                    }

                    handled = true;

                    try
                    {
                        await Commands.ExecuteCommandAsync(
                            Commands.CreateContext(
                                args.Context.Message,
                                CommandPrefix,
                                findCommand,
                                rawArguments));
                    }
                    catch { }

                    break;

                case ArgumentException:
                    if (args.Command != null)
                    {
                        handled = true;
                        handler.Message.Send(new ArgumentException("Wrong or invalid command parameter(s)"));
                    }
                    break;
            }

            await handler.LogError.SendAsync(
                string.Join(Environment.NewLine,
                    GetCommandInfo(args),
                    "Command errored",
                    args.Exception.GetExtendedMessage()));

            if (!handled)
            {
                handler.Message.Send(args.Exception);
            }
        }

        #endregion
    }
}
