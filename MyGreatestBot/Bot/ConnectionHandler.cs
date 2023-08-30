using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using MyGreatestBot.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.Bot
{
    [SupportedOSPlatform("windows")]
    internal sealed class ConnectionHandler
    {
        private const int SEND_MESSAGE_WAIT_MS = 1000;

        private readonly DiscordGuild Guild;
        internal readonly Player.Player PlayerInstance;

        internal volatile bool IsManualDisconnect;

        private static VoiceNextExtension? VoiceNext => BotWrapper.VoiceNext;

        internal VoiceNextConnection? VoiceConnection { get; set; }
        internal DiscordChannel? TextChannel { get; set; }
        internal DiscordChannel? VoiceChannel { get; set; }
        internal VoiceTransmitSink? TransmitSink { get; private set; }

        private static readonly Dictionary<ulong, ConnectionHandler> ConnectionDictionary = new();

        internal static ConnectionHandler? GetConnectionHandler(DiscordGuild guild)
        {
            if (guild == null)
            {
                return null;
            }
            ulong key = guild.Id;
            if (ConnectionDictionary.TryGetValue(key, out ConnectionHandler? handler))
            {
                return handler;
            }
            if (!ConnectionDictionary.TryAdd(key, new(guild)))
            {
                throw new ApplicationException("Cannot initialize connection handler");
            }
            return ConnectionDictionary[key];
        }

        internal void Update(DiscordGuild guild)
        {
            ConnectionDictionary[guild.Id] = this;
        }

        private ConnectionHandler(DiscordGuild guild)
        {
            Guild = guild;
            PlayerInstance = new(this);
        }

        internal ConnectionHandler()
        {
            throw new NotImplementedException();
        }

        private async Task GenericWriteLineAsync(TextWriter writer, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                await writer.WriteLineAsync($"{DateTime.Now:dd.MM.yyyy HH:mm:ss}\t{Guild.Name}{Environment.NewLine}{text}");
            }
        }

        internal async Task LogAsync(string text)
        {
            await GenericWriteLineAsync(Console.Out, text);
        }

        internal void Log(string text)
        {
            GenericWriteLineAsync(Console.Out, text).Wait();
        }

        internal async Task LogErrorAsync(string text)
        {
            await GenericWriteLineAsync(Console.Error, text);
        }

        internal void LogError(string text)
        {
            GenericWriteLineAsync(Console.Error, text).Wait();
        }

        internal VoiceNextConnection? GetVoiceConnection()
        {
            try
            {
                return VoiceNext?.GetConnection(Guild);
            }
            catch
            {
                return VoiceConnection;
            }
        }

        internal async Task WaitForConnectionAsync()
        {
            while (VoiceConnection == null)
            {
                VoiceConnection = GetVoiceConnection();
                await Task.Yield();
                await Task.Delay(1);
            }
        }

        internal async Task WaitForDisconnectionAsync()
        {
            while (VoiceConnection != null)
            {
                VoiceConnection = GetVoiceConnection();
                await Task.Yield();
                await Task.Delay(1);
            }
        }

        internal void Connect()
        {
            try
            {
                if (VoiceNext != null && VoiceConnection?.TargetChannel != VoiceChannel && VoiceChannel != null)
                {
                    _ = (VoiceNext?.ConnectAsync(VoiceChannel).Wait(1000));
                }
            }
            catch { }
        }

        internal void Disconnect()
        {
            IsManualDisconnect = true;
            try
            {
                VoiceConnection?.Disconnect();
                VoiceConnection?.Dispose();
                VoiceConnection = null;
            }
            catch { }
        }

        internal async Task Join(CommandContext ctx)
        {
            await Join(ctx.Channel, ctx.Member?.VoiceState?.Channel, true);
        }

        internal async Task Join(VoiceStateUpdateEventArgs args)
        {
            await Join(null, args.After?.Channel, false);
        }

        internal async Task Join(DiscordChannel? text, DiscordChannel? channel, bool throw_exception = false)
        {
            if (text != null)
            {
                TextChannel = text;
            }

            VoiceConnection = GetVoiceConnection();

            await Task.Yield();

            if (VoiceConnection != null && (VoiceChannel != channel || VoiceConnection.TargetChannel != channel))
            {
                Disconnect();
                WaitForDisconnectionAsync().Wait();
                await Task.Yield();
                if (channel != null)
                {
                    await Join(text, channel, throw_exception);
                }
                return;
            }

            if (channel != null)
            {
                VoiceChannel = channel;
                Connect();
                WaitForConnectionAsync().Wait();
            }
            else if (throw_exception)
            {
                throw new InvalidOperationException("You need to be in the voice channel");
            }

            await Task.Run(() => PlayerInstance.Resume(CommandActionSource.Mute));

            await Task.Delay(1);
        }

        internal static async Task Leave(CommandContext ctx)
        {
            ConnectionHandler? handler = GetConnectionHandler(ctx.Guild);

            if (handler == null)
            {
                return;
            }

            handler.PlayerInstance.Stop(CommandActionSource.Mute);

            handler.TextChannel = ctx.Channel;
            handler.VoiceConnection = handler.GetVoiceConnection();

            //if (VoiceConnection == null)
            //{
            //    throw new InvalidOperationException("Not connected in this guild.");
            //}

            handler.Disconnect();
            await handler.WaitForDisconnectionAsync();

            await Task.Delay(1);
        }

        internal void UpdateSink()
        {
            TransmitSink = VoiceConnection?.GetTransmitSink(PlayerInstance.TransmitSinkDelay);
        }

        internal async Task SendMessageAsync(DiscordEmbedBuilder embed)
        {
            if (TextChannel != null)
            {
                _ = await TextChannel.SendMessageAsync(embed);
            }
        }

        internal async Task SendMessageAsync(string message)
        {
            if (TextChannel != null)
            {
                _ = await TextChannel.SendMessageAsync(message);
            }
        }

        internal void SendMessage(DiscordEmbedBuilder embed)
        {
            _ = SendMessageAsync(embed).Wait(SEND_MESSAGE_WAIT_MS);
        }

        internal void SendMessage(string message)
        {
            _ = SendMessageAsync(message).Wait(SEND_MESSAGE_WAIT_MS);
        }

        internal void SendSpeaking(bool speaking)
        {
            try
            {
                _ = VoiceConnection?.SendSpeakingAsync(speaking).Wait(100);
            }
            catch { }
        }
    }
}
