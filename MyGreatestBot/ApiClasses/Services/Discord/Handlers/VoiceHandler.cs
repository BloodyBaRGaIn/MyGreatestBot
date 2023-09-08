﻿using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    public sealed class VoiceHandler
    {
        [AllowNull]
        private static VoiceNextExtension VoiceNext => DoscordWrapper.VoiceNext;
        [AllowNull]
        public VoiceNextConnection Connection { get; private set; }
        [AllowNull]
        public DiscordChannel Channel { get; private set; }
        [AllowNull]
        private VoiceTransmitSink TransmitSink { get; set; }

        private readonly DiscordGuild _guild;

        internal volatile bool IsManualDisconnect;

        public VoiceHandler(DiscordGuild guild)
        {
            _guild = guild;
        }

        public void UpdateVoiceConnection()
        {
            try
            {
                Connection = VoiceNext?.GetConnection(_guild);
            }
            catch { }
        }

        public async Task WaitForConnectionAsync()
        {
            while (Connection == null)
            {
                UpdateVoiceConnection();
                await Task.Yield();
                await Task.Delay(1);
            }
        }

        public async Task WaitForDisconnectionAsync()
        {
            while (Connection != null)
            {
                UpdateVoiceConnection();
                await Task.Yield();
                await Task.Delay(1);
            }
        }

        public void Connect(DiscordChannel channel)
        {
            Channel = channel;

            try
            {
                if (VoiceNext != null
                    && Channel != null
                    && Connection?.TargetChannel != Channel)
                {
                    _ = (VoiceNext?.ConnectAsync(Channel).Wait(1000));
                }
            }
            catch { }
        }

        public void Disconnect()
        {
            IsManualDisconnect = true;
            try
            {
                Connection?.Disconnect();
                Connection?.Dispose();
                Connection = null;
            }
            catch { }
        }

        public void SendSpeaking(bool speaking)
        {
            try
            {
                _ = Connection?.SendSpeakingAsync(speaking).Wait(100);
            }
            catch { }
        }

        public async Task WriteAsync(byte[] bytes)
        {
            while (TransmitSink == null)
            {
                UpdateSink();
                await Task.Delay(1);
                await Task.Yield();
            }

            await TransmitSink.WriteAsync(bytes);
        }

        public void UpdateSink()
        {
            TransmitSink = Connection?.GetTransmitSink(Player.Player.TransmitSinkDelay);
        }
    }
}
