using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Threading.Tasks;

using AllowNullAttribute = System.Diagnostics.CodeAnalysis.AllowNullAttribute;

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

        /// <summary>
        /// Update voice connection
        /// </summary>
        public void UpdateVoiceConnection()
        {
            try
            {
                Connection = VoiceNext?.GetConnection(guild);
            }
            catch { }
        }

        /// <summary>
        /// Awaiting for connection
        /// </summary>
        /// <returns></returns>
        public async Task WaitForConnectionAsync()
        {
            while (Connection == null)
            {
                await UpdateVoiceConnectionAsync();
            }
        }

        /// <summary>
        /// Awaiting for disconnection
        /// </summary>
        /// <returns></returns>
        public async Task WaitForDisconnectionAsync()
        {
            while (Connection != null)
            {
                await UpdateVoiceConnectionAsync();
            }
        }

        /// <summary>
        /// Update voice connection asynchronous
        /// </summary>
        private async Task UpdateVoiceConnectionAsync()
        {
            UpdateVoiceConnection();
            await Task.Yield();
            await Task.Delay(1);
        }

        /// <summary>
        /// Connect to specified channel
        /// </summary>
        /// <param name="channel">Channel to connect to</param>
        public void Connect(DiscordChannel channel)
        {
            try
            {
#pragma warning disable CS8604
                bool channel_changed = Connection?.TargetChannel != channel;
#pragma warning restore CS8604

                if (VoiceNext != null
                    && channel is not null
                    && channel_changed)
                {
                    _ = (VoiceNext?.ConnectAsync(channel).Wait(1000));
                    Channel = channel;
                }
            }
            catch { }
        }

        /// <summary>
        /// Disconnect from channel
        /// </summary>
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

        /// <summary>
        /// Send speaking status
        /// </summary>
        /// <param name="speaking">Speaking status</param>
        public void SendSpeaking(bool speaking)
        {
            try
            {
                _ = Connection?.SendSpeakingAsync(speaking).Wait(100);
            }
            catch { }
        }

        /// <summary>
        /// Write bytes array to the transmission sink
        /// </summary>
        /// <param name="bytes">Data to write</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get transmission sink
        /// </summary>
        public void UpdateSink()
        {
            TransmitSink = Connection?.GetTransmitSink(Player.Player.TransmitSinkDelay);
        }
    }
}
