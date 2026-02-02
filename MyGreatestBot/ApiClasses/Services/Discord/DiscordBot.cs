using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using MyGreatestBot.ApiClasses.ConfigClasses;
using MyGreatestBot.ApiClasses.ConfigClasses.JsonModels;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Commands;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
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
        /// Bot commands handling instance.
        /// </summary>
        [AllowNull] public CommandsNextExtension Commands { get; private set; }

        /// <summary>
        /// Bot's age. Zero if it's not its anniversary today.
        /// </summary>
        private int Age { get; set; } = -1;

        /// <summary>
        /// Actual bot's command prefix
        /// </summary>
        private string CommandPrefix = string.Empty;

        private volatile bool exitRequest;

        private string OnlineActivityName =>
            $"{(string.IsNullOrWhiteSpace(CommandPrefix) ? DiscordWrapper.DefaultPrefix : CommandPrefix)}{CommandStrings.HelpCommandName}";
        private DiscordActivityType OnlineActivityType { get; } = DiscordActivityType.ListeningTo;

        ApiIntents IAPI.ApiType => ApiIntents.Discord;
        ApiStatus IAPI.OldStatus { get; set; }

        bool IAPI.IsEssential => true;

        DomainCollection IAccessible.Domains { get; } = "http://www.discord.com/";

        void IAPI.PerformAuthInternal()
        {
            DiscordConfigJSON config_js = ConfigManager.GetDiscordConfigJSON();

            CommandPrefix = config_js.Prefix;

            DiscordClientBuilder clientBuilder =
                DiscordClientBuilder.CreateDefault(config_js.Token, DiscordIntents.All)
                .SetLogLevel(LogLevel.Debug)
                .UseCommandsNext(commands =>
                {
                    Commands = commands;

                    Commands.SetHelpFormatter<CustomHelpFormatter>();
                    Commands.RegisterCommands<ConnectionCommands>();
                    Commands.RegisterCommands<QueuingCommands>();
                    Commands.RegisterCommands<PlaybackCommands>();
                    Commands.RegisterCommands<DatabaseCommands>();
                    Commands.RegisterCommands<DebugCommands>();

                    Commands.CommandErrored += Commands_CommandErrored;
                    Commands.CommandExecuted += Commands_CommandExecuted;

                    MarkdownWriter.GenerateFile();
                }, new()
                {
                    StringPrefixes = [CommandPrefix],
                    CaseSensitive = false,
                    EnableMentionPrefix = true,
                    EnableDms = true,
                    EnableDefaultHelp = false,
                })
                .ConfigureEventHandlers(new Action<EventHandlingBuilder>(events =>
                {
                    _ = events.HandleSessionCreated(async (s, e) =>
                    {
                        if (s.CurrentUser != Client.CurrentUser)
                        {
                            return;
                        }

                        SetUserStatus(DiscordUserStatus.Online);

                        await Task.Delay(1);

                        DiscordWrapper.CurrentDomainLogHandler.Send("Session created.");

                        if (Age == -1)
                        {
                            (int age, bool bday) = CalculateAge();

                            if (age > 0)
                            {
                                Age = age;

                                if (bday)
                                {
                                    DiscordWrapper.CurrentDomainLogHandler.Send(
                                        $"It's my {Age} year anniversary today!!!");
                                }
                            }
                        }

                        await Task.Delay(1);
                    });
                    _ = events.HandleSocketOpened(async (s, e) =>
                    {
                        await DiscordWrapper.CurrentDomainLogHandler.SendAsync("SocketOpened");
                    });
                    _ = events.HandleSocketClosed(async (s, e) =>
                    {
                        if (s.CurrentUser != Client.CurrentUser)
                        {
                            return;
                        }
                        await DiscordWrapper.CurrentDomainLogHandler.SendAsync("SocketClosed");
                    });
                    _ = events.HandleVoiceStateUpdated(async (s, e) =>
                    {
                        if (s.CurrentUser != Client.CurrentUser)
                        {
                            return;
                        }

                        ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(await e.GetGuildAsync());
                        if (handler == null)
                        {
                            return;
                        }

                        static async Task<DiscordChannel?> GetFromState(DiscordVoiceState? state)
                        {
                            return state is null ? null : await state.GetChannelAsync();
                        }

                        static string GetName(DiscordChannel? channel)
                        {
                            return channel?.Name ?? "null";
                        }

                        DiscordChannel? beforeChannel = await GetFromState(e.Before);
                        DiscordChannel? afterChannel = await GetFromState(e.After);

                        await handler.Log.SendAsync(
                            $"VoiceStateUpdated from {GetName(beforeChannel)} to {GetName(afterChannel)}");
                    });
                    _ = events.HandleVoiceServerUpdated(async (s, e) =>
                    {
                        if (s.CurrentUser != Client.CurrentUser)
                        {
                            return;
                        }

                        ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(e.Guild);
                        if (handler == null)
                        {
                            return;
                        }

                        await handler.Log.SendAsync($"VoiceServerUpdated {e.Endpoint}");
                    });
                }))
                .UseVoiceNext(new VoiceNextConfiguration())
                .UseInteractivity(new InteractivityConfiguration()
                {
                    Timeout = TimeSpan.FromMinutes(10)
                });

            Client = clientBuilder.Build();

            if (string.IsNullOrWhiteSpace(CommandPrefix))
            {
                CommandPrefix = DiscordWrapper.DefaultPrefix;
                DiscordWrapper.CurrentDomainLogErrorHandler.Send("Command prefix set to its default value", LogLevel.Warning);
            }
        }

        void IAPI.LogoutInternal()
        {
            if (Commands == null)
            {
                return;
            }

            if (Commands.RegisteredCommands.Count != 0)
            {
                try
                {
                    Commands.UnregisterCommands(cmds: [.. Commands.RegisteredCommands.Values]);
                }
                catch { }
            }

            Commands.CommandExecuted -= Commands_CommandExecuted;
            Commands.CommandErrored -= Commands_CommandErrored;
        }

        private async Task ExecuteCommandAsync(params string[] @params)
        {
            if (Client == null)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    "Bot is not initialized.");
                return;
            }

            if (@params.Length < 1)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    "Invalid parameters.");
                return;
            }

            _ = new ConsoleCommands().InvokeMethod(@params[0], (@params.Length > 1) ? @params[1..] : null);

            await Task.Yield();
        }

        /// <summary>
        /// Runs bot
        /// </summary>
        internal void Run()
        {
            // try to start
            try
            {
                if (Client == null)
                {
                    throw new DiscordApiException();
                }

                if (!Client.ConnectAsync().Wait(DiscordWrapper.ConnectionTimeout))
                {
                    throw new DiscordApiException("Cannot connect to Discord");
                }
            }
            catch (Exception ex)
            {
                Disconnect();

                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    string.Join(Environment.NewLine,
                        ex.GetExtendedMessage(),
                        "Press any key to exit"));

                _ = Console.ReadKey(true);
                return;
            }

            using SharedClasses.NonBlockingConsole console = new();
            console.Start();

            using Task waitForExitTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Name = nameof(DiscordBot);

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

                    if (!console.DequeueString(out string? in_str))
                    {
                        continue;
                    }

                    string[] in_split = in_str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (in_split == null || in_split.Length == 0)
                    {
                        continue;
                    }

                    try
                    {
                        await ExecuteCommandAsync(in_split);
                    }
                    catch (Exception ex)
                    {
                        DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                            ex.GetExtendedMessage());
                    }
                }
            });

            waitForExitTask.Wait();

            try
            {
                console.Dispose();
            }
            catch { }

            if (Client == null)
            {
                return;
            }

            // try to set offline status
            SetUserStatus(DiscordUserStatus.Offline);

            Disconnect();
        }

        internal (int, bool) CalculateAge()
        {
            if (Client == null)
            {
                return (-1, false);
            }

            DateTime birthdate = Client.CurrentUser.CreationTimestamp.Date;

            DateTime today = DateTime.Today;

            int age = today.Year - birthdate.Year;

            while (birthdate.Date > today.AddYears(-age))
            {
                age--;
            }

            return (age, birthdate.Month == today.Month && birthdate.Day == today.Day);
        }

        private void SetUserStatus(DiscordUserStatus status)
        {
            if (Client == null)
            {
                return;
            }

            DiscordActivity activity;
            int timeout;

            switch (status)
            {
                case DiscordUserStatus.Online:
                    activity = new(OnlineActivityName, OnlineActivityType);
                    timeout = DiscordWrapper.ConnectionTimeout;
                    break;

                case DiscordUserStatus.Offline:
                default:
                    activity = new();
                    timeout = DiscordWrapper.DisconnectionTimeout;
                    break;
            }

            try
            {
                _ = Client.UpdateStatusAsync(activity, status).Wait(timeout);
            }
            catch { }
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

            string parameters;

            if (args.Command != null)
            {
                result += $"{args.Command.Name} ";
                parameters = args.Context.RawArgumentString;
            }
            else
            {
                parameters = args.Context.Message.Content;
            }

            if (!string.IsNullOrWhiteSpace(parameters))
            {
                result += parameters;
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
        private void Disconnect()
        {
            if (Client == null)
            {
                return;
            }
            try
            {
                _ = Client.DisconnectAsync().Wait(DiscordWrapper.DisconnectionTimeout);
            }
            catch { }
            try
            {
                Client.Dispose();
            }
            catch { }
        }

        #region Private event handlers

        //        private async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        //        {
        //            string eventName = $"{nameof(Client.VoiceStateUpdated)} {e.After?.Channel?.Name ?? "null"}";

        //            bool isBotTriggered = e.User.Id == client.CurrentUser.Id && e.User.IsBot;
        //            if (!isBotTriggered)
        //            {
        //                return;
        //            }

        //            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(e.Guild);
        //            if (handler == null)
        //            {
        //                return;
        //            }

        //            handler.Log.Send($"{eventName} {VoiceEventState.Entry}", LogLevel.Debug);

        //            bool semaphoreReady;

        //#pragma warning disable CS8604
        //            bool channel_changed = (e.After?.Channel) != (e.Before?.Channel);
        //#pragma warning restore CS8604
        //            // TODO sometimes event triggering without reason
        //            if (!channel_changed)
        //            {
        //                if (e.After?.Channel is not null)
        //                {
        //                    semaphoreReady = handler.VoiceUpdateSemaphore.TryWaitOne(10000);

        //                    if (semaphoreReady)
        //                    {
        //                        handler.VoiceUpdating = true;

        //                        handler.Voice.Disconnect();
        //                        await handler.Join(e);
        //                        await handler.Voice.WaitForConnectionAsync();

        //                        handler.Log.Send($"{eventName} {VoiceEventState.FastFinish}", LogLevel.Debug);

        //                        handler.VoiceUpdating = false;

        //                        _ = handler.VoiceUpdateSemaphore.TryRelease();
        //                    }
        //                }
        //                return;
        //            }

        //            handler.Log.Send($"{eventName} {VoiceEventState.ChannelChanged}", LogLevel.Debug);

        //            if (handler.Voice.IsManualDisconnect)
        //            {
        //                await Task.Yield();
        //                return;
        //            }

        //            handler.Log.Send($"{eventName} {VoiceEventState.Start}", LogLevel.Debug);

        //            await Task.Yield();

        //            semaphoreReady = handler.VoiceUpdateSemaphore.TryWaitOne(10000);

        //            await Task.Run(async () =>
        //            {
        //                if (semaphoreReady)
        //                {
        //                    if (handler.VoiceUpdating)
        //                    {
        //                        handler.Log.Send($"{eventName} {VoiceEventState.InProgress}", LogLevel.Debug);
        //                        await Task.Delay(1);
        //                        return;
        //                    }

        //                    handler.VoiceUpdating = true;

        //                    using Task waitConnectionTask = Task.Run(async () =>
        //                    {
        //                        Thread.CurrentThread.Name = $"{nameof(waitConnectionTask)} {handler.GuildName}";

        //                        Stopwatch? stopwatch = null;

        //                        while (true)
        //                        {
        //                            stopwatch ??= Stopwatch.StartNew();
        //                            if (stopwatch.ElapsedMilliseconds > 5000)
        //                            {
        //                                break;
        //                            }
        //                            handler.Voice.UpdateVoiceConnection();
        //                            if (handler.Voice.Connection != null)
        //                            {
        //                                break;
        //                            }
        //                            await Task.Delay(20);
        //                            await Task.Yield();
        //                        }

        //                        stopwatch?.Stop();
        //                    });

        //                    await waitConnectionTask;

        //                    if (handler.VoiceConnection == null)
        //                    {
        //                        if (e.After?.Channel is null)
        //                        {
        //                            await Task.Run(() => handler.PlayerInstance.Stop(CommandActionSource.Event | CommandActionSource.Mute));
        //                            handler.Message.Send(new DiscordEmbedBuilder()
        //                            {
        //                                Color = DiscordColor.Red,
        //                                Title = "Kicked from voice channel"
        //                            });
        //                            handler.Voice.Disconnect(false);
        //                        }
        //                        else
        //                        {
        //                            handler.LogError.Send("Cannot update voice state");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (e.After?.Channel is not null)
        //                        {
        //                            handler.Voice.IsManualDisconnect = true;
        //                            await handler.Join(e);
        //                            await handler.Voice.WaitForConnectionAsync();
        //                        }
        //                        else
        //                        {
        //                            handler.LogError.Send("Voice state is illegal");
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    handler.Log.Send($"{eventName} {VoiceEventState.Busy}", LogLevel.Debug);
        //                }

        //                handler.Update(e.Guild);
        //            });

        //            if (semaphoreReady)
        //            {
        //                handler.Log.Send($"{eventName} {VoiceEventState.Finish}", LogLevel.Debug);

        //                handler.VoiceUpdating = false;

        //                _ = handler.VoiceUpdateSemaphore.TryRelease();
        //            }

        //            await Task.Yield();
        //        }

        //private async Task Client_VoiceServerUpdated(DiscordClient client, VoiceServerUpdateEventArgs e)
        //{
        //    string eventName = $"{nameof(Client.VoiceServerUpdated)} {e.Endpoint ?? "null"}";

        //    bool isBotTriggered = Client.CurrentUser.Id == client.CurrentUser.Id;
        //    if (!isBotTriggered)
        //    {
        //        return;
        //    }

        //    ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(e.Guild);
        //    if (handler == null)
        //    {
        //        return;
        //    }

        //    handler.Log.Send($"{eventName} {VoiceEventState.Entry}", LogLevel.Debug);

        //    bool not_changed = false;

        //    if (handler.Voice.Endpoint != e.Endpoint || handler.Voice.Token != e.VoiceToken)
        //    {
        //        if (string.IsNullOrWhiteSpace(handler.Voice.Endpoint) ||
        //            string.IsNullOrWhiteSpace(e.Endpoint) ||
        //            string.IsNullOrWhiteSpace(handler.Voice.Token) ||
        //            string.IsNullOrWhiteSpace(e.VoiceToken))
        //        {
        //            not_changed = true;
        //        }
        //        handler.Voice.Endpoint = e.Endpoint;
        //        handler.Voice.Token = e.VoiceToken;
        //    }

        //    if (not_changed || handler.Voice.IsManualDisconnect)
        //    {
        //        await Task.Yield();
        //        return;
        //    }

        //    handler.Log.Send($"{eventName} {VoiceEventState.Start}", LogLevel.Debug);

        //    try
        //    {
        //        await Task.Delay(5000);
        //    }
        //    catch { }

        //    bool semaphoreReady = handler.VoiceUpdateSemaphore.TryWaitOne(10000);
        //    if (semaphoreReady)
        //    {
        //        if (handler.VoiceUpdating || handler.ServerUpdating)
        //        {
        //            handler.Log.Send($"{eventName} {VoiceEventState.InProgress}", LogLevel.Debug);
        //            _ = handler.VoiceUpdateSemaphore.TryRelease();
        //            return;
        //        }

        //        handler.VoiceUpdating = true;
        //        handler.ServerUpdating = true;

        //        try
        //        {
        //            handler.Voice.IsManualDisconnect = true;
        //            await handler.Reconnect();
        //        }
        //        catch (Exception ex)
        //        {
        //            handler.Log.Send(ex.GetExtendedMessage());
        //        }

        //        _ = handler.VoiceUpdateSemaphore.TryRelease();

        //        handler.Log.Send($"{eventName} {VoiceEventState.Finish}", LogLevel.Debug);

        //        handler.ServerUpdating = false;
        //        handler.VoiceUpdating = false;
        //    }
        //    else
        //    {
        //        handler.Log.Send($"{eventName} {VoiceEventState.Busy}", LogLevel.Debug);
        //    }

        //    handler.Update(e.Guild);

        //    await Task.Delay(1);
        //}

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

            handler.TextChannel = args.Context.Channel;

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

        private enum VoiceEventState
        {
            Entry,
            ChannelChanged,
            Start,
            InProgress,
            Busy,
            Finish,
            FastFinish
        }

        #endregion
    }
}
