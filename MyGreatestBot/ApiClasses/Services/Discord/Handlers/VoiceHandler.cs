using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Voice connection handler class
    /// </summary>
    public sealed class VoiceHandler(DiscordGuild guild)
    {
        [AllowNull]
        private static VoiceNextExtension VoiceNext => DiscordWrapper.VoiceNext;
        [AllowNull]
        public VoiceNextConnection Connection { get; private set; }
        [AllowNull]
        public DiscordChannel Channel { get; private set; }
        [AllowNull]
        private VoiceTransmitSink TransmitSink { get; set; }

        internal volatile bool IsManualDisconnect;

        public void UpdateVoiceConnection()
        {
            try
            {
                Connection = VoiceNext?.GetConnection(guild);
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
            try
            {
                if (VoiceNext != null
                    && channel is not null
                    && Connection?.TargetChannel != channel)
                {
                    _ = (VoiceNext?.ConnectAsync(channel).Wait(1000));
                    Channel = channel;
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
