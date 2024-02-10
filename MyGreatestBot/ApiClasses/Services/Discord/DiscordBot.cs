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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace MyGreatestBot.ApiClasses.Services.Discord
{
    public sealed class DiscordBot : IAPI, IAccessible
    {
        [AllowNull]
        public DiscordClient Client { get; private set; }
        [AllowNull]
        public InteractivityExtension Interactivity { get; private set; }
        [AllowNull]
        public CommandsNextExtension Commands { get; private set; }
        [AllowNull]
        public VoiceNextExtension Voice { get; private set; }

        private ServiceProvider ServiceProvider { get; } = new ServiceCollection().BuildServiceProvider();

        private string[]? StringPrefixes;

        private volatile bool exitRequest;

        ApiIntents IAPI.ApiType => ApiIntents.Discord;

        DomainCollection IAccessible.Domains { get; } = new("http://www.discord.com/");

        public void PerformAuth()
        {
            DiscordConfigJSON config_js = ConfigManager.GetDiscordConfigJSON();

            DiscordConfiguration discordConfig = new()
            {
                MinimumLogLevel = LogLevel.Error,
                Intents = DiscordIntents.All,
                Token = config_js.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);

            Client.SessionCreated += async (sender, args) =>
            {
                await sender.UpdateStatusAsync(new()
                {
                    ActivityType = ActivityType.ListeningTo,
                    Name = $"{config_js.Prefix}{CommandStrings.HelpCommandName}"
                }, UserStatus.Online);

                DiscordWrapper.CurrentDomainLogHandler.Send("Discord ONLINE");
            };

            Client.ClientErrored += async (sender, args) =>
            {
                await DiscordWrapper.CurrentDomainLogErrorHandler.SendAsync(
                    args.Exception.GetExtendedMessage());
            };

            Client.SocketErrored += async (sender, args) =>
            {
                await DiscordWrapper.CurrentDomainLogErrorHandler.SendAsync(
                    args.Exception.GetExtendedMessage());
            };

            Client.SocketClosed += async (sender, args) =>
            {
                await DiscordWrapper.CurrentDomainLogErrorHandler.SendAsync(
                    args.CloseMessage);
            };

            Client.VoiceStateUpdated += Client_VoiceStateUpdated;

            Interactivity = Client.UseInteractivity(new()
            {
                Timeout = TimeSpan.FromMinutes(20)
            });

            StringPrefixes = [config_js.Prefix];

            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = StringPrefixes,
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
                    $"{ex.GetExtendedMessage()}{Environment.NewLine}Press any key to exit");
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
                    new(), UserStatus.Offline)
                .Wait(disconnectionTimeout);
            }
            catch { }

            Disconnect(disconnectionTimeout);
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
        /// <param name="ms">Timeout</param>
        private void Disconnect(int ms)
        {
            if (Client == null)
            {
                return;
            }
            try
            {
                _ = Client.DisconnectAsync().Wait(ms);
                Client.Dispose();
            }
            catch { }
        }

        private async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
#pragma warning disable CS8604
            bool channel_changed = (e.After?.Channel) != (e.Before?.Channel);
#pragma warning restore CS8604

            if (e.User != client.CurrentUser || !e.User.IsBot || !channel_changed)
            {
                return;
            }
            try
            {
                Thread.BeginCriticalRegion();
                ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(e.Guild);
                if (handler != null)
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
                            handler.PlayerInstance.Stop(CommandActionSource.Event | CommandActionSource.Mute);
                            handler.Message.Send(new DiscordEmbedBuilder()
                            {
                                Color = DiscordColor.Red,
                                Title = "Kicked from voice channel"
                            });
                        }
                        handler.Voice.IsManualDisconnect = false;
                    }

                    handler.Update(e.Guild);
                }
                Thread.EndCriticalRegion();
            }
            catch { }
            finally
            {
                await Task.Delay(1);
            }
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

                    if (StringPrefixes == null || StringPrefixes.Length == 0)
                    {
                        return;
                    }

                    string? badCommandText = args.Context.Message.Content;

                    if (badCommandText == null)
                    {
                        return;
                    }

                    foreach (string prefix in StringPrefixes)
                    {
                        if (badCommandText.StartsWith(prefix))
                        {
                            badCommandText = badCommandText[prefix.Length..];
                            break;
                        }
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

                    if (findCommand != null)
                    {
                        handled = true;
                        try
                        {
                            await Commands.ExecuteCommandAsync(
                                Commands.CreateContext(
                                    args.Context.Message,
                                    StringPrefixes[0],
                                    findCommand,
                                    rawArguments));
                        }
                        catch { }
                    }
                    break;

                case ArgumentException:
                    if (args.Command != null)
                    {
                        handled = true;
                        handler.Message.Send(new ArgumentException("Wrong or invalid command parameter(s)"));
                    }
                    break;
            }

            await handler.LogError.SendAsync($"{GetCommandInfo(args)}{Environment.NewLine}" +
                $"Command errored{Environment.NewLine}" +
                $"{args.Exception.GetExtendedMessage()}");

            if (!handled)
            {
                handler.Message.Send(args.Exception);
            }
        }
    }
}
