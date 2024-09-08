global using ConnectionHandler = MyGreatestBot.ApiClasses.Services.Discord.Handlers.ConnectionHandler;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Connection handler class
    /// </summary>
    public sealed class ConnectionHandler
    {
        [AllowNull] private readonly Player.Player _player = null;

        private readonly PlayerException playerNotInitializedException = new("Not initialized");

        internal Player.Player PlayerInstance => _player ?? throw playerNotInitializedException;
        internal bool IsPlayerInitialized => _player != null;

        private readonly DiscordGuild _guild;

        public ulong GuildId => _guild.Id;
        public string GuildName => _guild.Name;

        private bool FirstTimeTextChannelAssigned = true;

        public DiscordChannel? TextChannel
        {
            get => Message.Channel;
            set
            {
                Message.Channel = value;
                if (value is null || !FirstTimeTextChannelAssigned)
                {
                    return;
                }
                FirstTimeTextChannelAssigned = false;
                if (DiscordWrapper.Age <= 0)
                {
                    return;
                }
                Message.Send(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.White,
                    Title = "Anniversary",
                    Description =
                        $":partying_face:" +
                        $"It's my {DiscordWrapper.Age} year anniversary today!!!" +
                        $":partying_face:"
                });
            }
        }

        [AllowNull] public DiscordChannel VoiceChannel => Voice.Channel;
        [AllowNull] public VoiceNextConnection VoiceConnection => Voice.Connection;

        public LogHandler Log { get; }
        public LogHandler LogError { get; }
        public MessageHandler Message { get; }
        public VoiceHandler Voice { get; }

        public Semaphore VoiceUpdateSemaphore { get; } = new(1, 1);
        public Semaphore ServerUpdateSemaphore { get; } = new(1, 1);

        public volatile bool VoiceUpdating = false;
        public volatile bool ServerUpdating = false;

        private static readonly Dictionary<ulong, ConnectionHandler> ConnectionDictionary = [];

        private ConnectionHandler(DiscordGuild guild)
        {
            _guild = guild;

            Log = new(
                writer: Console.Out,
                guildName: guild.Name,
                logDelay: 1000,
                defaultLogLevel: LogLevel.Information);

            LogError = new(
                writer: Console.Error,
                guildName: guild.Name,
                logDelay: 1000,
                defaultLogLevel: LogLevel.Error);

            Message = new(messageDelay: 1000);

            Voice = new(guild);

            try
            {
                _player = new(this);
            }
            catch (PlayerException ex)
            {
                playerNotInitializedException = ex;
            }
            catch
            {
                throw;
            }
        }

        public static ConnectionHandler? GetConnectionHandler(DiscordGuild guild)
        {
            if (guild is null)
            {
                return null;
            }

            ulong key = guild.Id;

            return ConnectionDictionary.TryGetValue(key, out ConnectionHandler? handler)
                ? handler
                : ConnectionDictionary.TryAdd(key, new(guild))
                ? ConnectionDictionary[key]
                : throw new ApplicationException("Cannot initialize connection handler");
        }

        public void Update(DiscordGuild guild)
        {
            ConnectionDictionary[guild.Id] = this;
        }

        public async Task Join(CommandContext ctx)
        {
            await Join(ctx.Channel, ctx.Member?.VoiceState?.Channel, true);
        }

        public async Task Join(VoiceStateUpdateEventArgs args)
        {
            await Join(null, args.After?.Channel, false);
        }

        private async Task Join(
            DiscordChannel? text,
            DiscordChannel? channel, bool throw_exception = false)
        {
            if (text is not null)
            {
                TextChannel = text;
            }

            if (!IsPlayerInitialized)
            {
                throw playerNotInitializedException;
            }

            Voice.UpdateVoiceConnection();

            await Task.Yield();

            DiscordChannel? new_channel = null;
            DiscordChannel? old_channel = null;

            if (VoiceChannel is not null)
            {
                old_channel = await _guild.GetChannelAsync(VoiceChannel.Id);
            }
            if (channel is not null)
            {
                new_channel = await _guild.GetChannelAsync(channel.Id);
            }

            bool connection_rollback = false;

            if (channel is not null && !channel.PermissionsFor(_guild.CurrentMember)
                .HasFlag(DiscordPermissions.AccessChannels |
                         DiscordPermissions.UseVoice |
                         DiscordPermissions.Speak))
            {
                connection_rollback = true;
                new_channel = old_channel;
            }

#pragma warning disable CS8604
            bool channel_changed = VoiceChannel != channel || VoiceConnection?.TargetChannel != channel;
#pragma warning restore CS8604

            if (VoiceConnection != null && channel_changed)
            {
                await Task.Run(() => PlayerInstance.Pause(CommandActionSource.Mute));
                await Task.Delay(40);
                //Voice.Disconnect();
                //Voice.WaitForDisconnectionAsync().Wait();
                await Task.Delay(200);
            }

            if (new_channel is not null)
            {
                Voice.Connect(new_channel);
                Voice.WaitForConnectionAsync().Wait();
                Voice.SendSpeaking(false);
                await Task.Delay(40);
                await Task.Run(() => PlayerInstance.Resume(CommandActionSource.Mute));
            }
            else if (!connection_rollback)
            {
                InvalidOperationException exception = new("You need to be in the voice channel");
                if (throw_exception)
                {
                    throw exception;
                }
                else
                {
                    Message.Send(exception);
                }
            }

            if (connection_rollback)
            {
                InvalidOperationException exception = new("Cannot join this channel");
                if (throw_exception)
                {
                    throw exception;
                }
                else
                {
                    Message.Send(exception);
                }
            }

            Voice.UpdateVoiceConnection();

            await Task.Delay(1);
        }

        public async Task Leave(CommandContext ctx)
        {
            await Leave(ctx.Channel, ctx.Member?.VoiceState?.Channel);
        }

        private async Task Leave(
            [AllowNull] DiscordChannel text,
            [AllowNull] DiscordChannel channel)
        {
            if (text is not null)
            {
                TextChannel = text;
            }

            if (VoiceChannel is null)
            {
                return;
            }

#pragma warning disable CS8604
            if ((VoiceChannel != channel && _guild == channel?.Guild)
                || (channel is null && _guild == TextChannel?.Guild))
#pragma warning restore CS8604
            {
                throw new InvalidOperationException("You need to be in the same voice channel");
            }

            await Task.Run(() => PlayerInstance.Stop(CommandActionSource.Mute));

            Voice.UpdateVoiceConnection();
            Voice.Disconnect();

            await Voice.WaitForDisconnectionAsync();
            await Task.Delay(1);
        }

        public async Task Reconnect()
        {
            DiscordChannel? origin = VoiceConnection?.TargetChannel ?? VoiceChannel;
            if (origin is null)
            {
                return;
            }

            DiscordChannel? channel = await _guild.GetChannelAsync(origin.Id);

            if (channel is null)
            {
                return;
            }

            await Task.Run(() => PlayerInstance.Pause(CommandActionSource.Event | CommandActionSource.Mute));

            try
            {
                Voice.Disconnect();
                await Voice.WaitForDisconnectionAsync();
                await Task.Yield();
                Voice.Connect(channel);
                await Voice.WaitForConnectionAsync();
                Voice.SendSpeaking(false);
                await Task.Yield();
            }
            catch (Exception ex)
            {
                Message.Send(ex);
            }

            await Task.Run(() => PlayerInstance.Resume(CommandActionSource.Event | CommandActionSource.Mute));

            Voice.UpdateVoiceConnection();
            await Task.Delay(1);
        }

        public static async Task Logout(CommandActionSource source = CommandActionSource.LogoutExit)
        {
            ParallelLoopResult result = Parallel.ForEach(ConnectionDictionary.Values, (handler) =>
            {
                if (source.HasFlag(CommandActionSource.LogoutExit))
                {
                    try
                    {
                        handler.Message.Send(new DiscordEmbedBuilder()
                            .WithTitle("Application is closing")
                            .WithColor(DiscordColor.Red));
                    }
                    catch { }
                }
                else if (source.HasFlag(CommandActionSource.LogoutShut))
                {
                    try
                    {
                        handler.Message.Send(new DiscordEmbedBuilder()
                            .WithTitle("Shutting down")
                            .WithColor(DiscordColor.Red));
                    }
                    catch { }
                }
                else
                {
                    source = CommandActionSource.LogoutBye;
                    try
                    {
                        handler.Message.Send(":wave:");
                    }
                    catch { }
                }

                try
                {
                    Task terminateTask = Task.Run(() => handler.PlayerInstance.Terminate(CommandActionSource.Command));
                    if (source.HasFlag(CommandActionSource.LogoutShut))
                    {
                        _ = terminateTask.Wait(1);
                    }
                    else
                    {
                        terminateTask.Wait();
                    }
                }
                catch { }

                try
                {
                    handler.Leave(handler.TextChannel, handler.VoiceChannel).Wait();
                }
                catch { }
            });

            while (!result.IsCompleted)
            {
                await Task.Delay(1);
            }

            try
            {
                DiscordWrapper.Logout();
            }
            catch { }

            DiscordWrapper.Exit();
            await Task.Yield();
        }
    }
}
