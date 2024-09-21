using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using MyGreatestBot.Player;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Voice connection handler class
    /// </summary>
    public sealed class VoiceHandler(DiscordGuild guild) : IDisposable
    {
        [AllowNull] private static VoiceNextExtension VoiceNext => DiscordWrapper.VoiceNext;
        [AllowNull] public DiscordChannel Channel { get; private set; }
        [AllowNull] private VoiceTransmitSink TransmitSink { get; set; }
        [AllowNull] private DiscordChannel LastKnownChannel { get; set; } = null;

        [AllowNull]
        public VoiceNextConnection Connection
        {
            get => _connection;
            private set
            {
                if (value is null)
                {
                    LastKnownChannel = null;
                }
                else if (value.TargetChannel is not null)
                {
                    LastKnownChannel = value.TargetChannel;
                }
                _connection = value;
            }
        }

        [AllowNull] private VoiceNextConnection _connection;

        internal volatile bool IsManualDisconnect;

        private bool disposed;

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
            do
            {
                await UpdateVoiceConnectionAsync();
            } while (Connection != null);
        }

        /// <summary>
        /// Update voice connection asynchronous
        /// </summary>
        private async Task UpdateVoiceConnectionAsync()
        {
            UpdateVoiceConnection();
            await Task.Delay(1);
        }

        /// <summary>
        /// Connect to specified channel
        /// </summary>
        /// <param name="channel">Channel to connect to</param>
        public void Connect(DiscordChannel? channel)
        {
            try
            {
                bool channel_changed = Channel != channel || Connection?.TargetChannel != channel;
                while (true)
                {
                    if (channel_changed)
                    {
                        Disconnect();
                        WaitForDisconnectionAsync().Wait();
                    }

                    if (VoiceNext != null
                        && channel is not null
                        && channel_changed)
                    {
                        Task<VoiceNextConnection> task = VoiceNext.ConnectAsync(channel);
                        _ = task.Wait(2000);
                        Connection = task.IsCompletedSuccessfully ? task.Result : null;
                    }

                    if (Connection != null)
                    {
                        break;
                    }
                }
            }
            catch { }

            Channel = channel;
            IsManualDisconnect = false;
        }

        /// <summary>
        /// Disconnect from channel
        /// </summary>
        public void Disconnect(bool isManual = true)
        {
            IsManualDisconnect = isManual;
            try
            {
                SendSpeaking(false);
                Connection?.Pause();
                _ = Connection?.WaitForPlaybackFinishAsync()?.Wait(PlayerHandler.TransmitSinkDelay * 2);
                Connection?.Disconnect();
                Connection?.Dispose();
                Connection = null;
                Channel = null;
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
                _ = Connection?.SendSpeakingAsync(speaking).Wait(1000);
            }
            catch { }
        }

        /// <summary>
        /// Write bytes array to the transmission sink
        /// </summary>
        /// <param name="bytes">Data to write</param>
        /// <param name="cnt">Count of bytes to write</param>
        /// <returns>Written bytes count</returns>
        public async Task<int> WriteAsync(byte[] bytes, int cnt, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return 0;
                }

                UpdateSink();

                if (TransmitSink != null)
                {
                    break;
                }

                Task delayTask = Task.Delay(1, cancellationToken);
                await delayTask;
                if (delayTask.IsCompletedSuccessfully)
                {
                    break;
                }
                else
                {
                    return 0;
                }
            }

            if (cancellationToken.IsCancellationRequested || TransmitSink == null)
            {
                return 0;
            }

            await Connection.ResumeAsync();

            if (cnt == 0)
            {
                cnt = bytes.Length;
            }

            ReadOnlyMemory<byte> buffer = (cnt == bytes.Length) ? bytes : bytes.AsMemory(0, cnt);

            if (Connection.TargetChannel is not null)
            {
                if (LastKnownChannel is null)
                {
                    LastKnownChannel = Connection.TargetChannel;
                }
                else if (LastKnownChannel != Connection.TargetChannel)
                {
                    return -1;
                }
            }

            Task writeTask = TransmitSink.WriteAsync(buffer, cancellationToken);
            await writeTask;
            return writeTask.IsCompletedSuccessfully ? cnt : 0;
        }

        /// <summary>
        /// Get transmission sink
        /// </summary>
        public void UpdateSink()
        {
            TransmitSink = Connection?.GetTransmitSink(PlayerHandler.TransmitSinkDelay);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            if (disposing)
            {
                ;
            }
            Disconnect();
        }
    }
}
