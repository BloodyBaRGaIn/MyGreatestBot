using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Connection handler class
    /// </summary>
    public sealed class ConnectionHandler
    {
        [AllowNull]
        private readonly Player.Player _player = null;
        private readonly PlayerException playerNotInitializedException = new();

        internal Player.Player PlayerInstance => _player ?? throw playerNotInitializedException;
        internal bool IsPlayerInitialized => _player != null;

        private readonly DiscordGuild _guild;

        public ulong GuildId => _guild.Id;

        public DiscordChannel? TextChannel
        {
            get => Message.Channel;
            set => Message.Channel = value;
        }

        [AllowNull]
        public DiscordChannel VoiceChannel => Voice.Channel;
        [AllowNull]
        public VoiceNextConnection VoiceConnection => Voice.Connection;

        public LogHandler Log { get; }
        public LogHandler LogError { get; }
        public MessageHandler Message { get; }
        public VoiceHandler Voice { get; }

        private static readonly Dictionary<ulong, ConnectionHandler> ConnectionDictionary = [];

        private ConnectionHandler(DiscordGuild guild)
        {
            _guild = guild;
            Log = new(writer: Console.Out, guildName: guild.Name);
            LogError = new(writer: Console.Error, guildName: guild.Name);
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
            [AllowNull] DiscordChannel text,
            [AllowNull] DiscordChannel channel, bool throw_exception = false)
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

            if (VoiceConnection != null
                && (VoiceChannel != channel
                    || VoiceConnection.TargetChannel != channel))
            {
                Voice.Disconnect();
                Voice.WaitForDisconnectionAsync().Wait();
                await Task.Yield();
                //if (channel != null)
                //{
                //    await Join(text, channel, throw_exception);
                //}
                //return;
            }

            if (channel is not null)
            {
                Voice.Connect(channel);
                Voice.WaitForConnectionAsync().Wait();
            }
            else if (throw_exception)
            {
                throw new InvalidOperationException("You need to be in the voice channel");
            }

            await Task.Run(() => PlayerInstance.Resume(CommandActionSource.Mute));

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

            if ((VoiceChannel != channel && _guild == channel?.Guild)
                || (channel is null && _guild == TextChannel?.Guild))
            {
                throw new InvalidOperationException("You need to be in the same voice channel");
            }

            PlayerInstance.Stop(CommandActionSource.Mute);

            Voice.UpdateVoiceConnection();
            Voice.Disconnect();

            await Voice.WaitForDisconnectionAsync();

            await Task.Delay(1);
        }

        public static async Task Logout(bool wave = true)
        {
            try
            {
                DiscordWrapper.Commands?.UnregisterCommands(
                    DiscordWrapper.Commands?.RegisteredCommands.Values.ToArray() ?? []);
            }
            catch { }

            ParallelLoopResult result = Parallel.ForEach(ConnectionDictionary.Values, async (handler) =>
            {
                try
                {
                    handler.PlayerInstance.Terminate(CommandActionSource.Command);
                }
                catch { }

                try
                {
                    await handler.Leave(null, null);
                }
                catch { }

                if (wave)
                {
                    await handler.Message.SendAsync(":wave:");
                }
            });

            while (!result.IsCompleted)
            {
                await Task.Delay(1);
            }

            DSharpPlus.DiscordClient? bot_client = DiscordWrapper.Client;

            // NullReferenceException could be thrown
            try
            {
                if (bot_client != null)
                {
                    lock (bot_client)
                    {
                        Task disconnectTask = Task.Run(() =>
                        {
                            try
                            {
                                _ = bot_client.DisconnectAsync();
                            }
                            catch { }
                        });
                        Task offlineTask = Task.Run(() =>
                        {
                            try
                            {
                                _ = bot_client.UpdateStatusAsync(new DiscordActivity("Offline"), UserStatus.Offline);
                            }
                            catch { }
                        });
                        try
                        {
                            Task.WaitAll(disconnectTask, offlineTask);
                        }
                        catch { }
                    }
                    bot_client.Dispose();
                }
            }
            catch { }

            ApiManager.DeinitApis();

            Environment.Exit(0);
        }
    }
}
