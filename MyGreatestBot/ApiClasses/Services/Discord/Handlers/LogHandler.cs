global using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using MyGreatestBot.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Console logging class
    /// </summary>
    public sealed class LogHandler(TextWriter writer,
                                   string guildName,
                                   int logDelay,
                                   LogLevel defaultLogLevel = LogLevel.None) : IDisposable
    {
        public const string DateTimeFormat = "dd.MM.yyyy HH:mm:ss.fff";

        private static readonly Semaphore writerSemaphore = new(1, 1);

        private async Task GenericWriteLineAsync(string text, LogLevel logLevel)
        {
            if (string.IsNullOrWhiteSpace(text) || writer == null)
            {
                return;
            }
            if (!writerSemaphore.TryWaitOne(logDelay))
            {
                return;
            }
            lock (writer)
            {
                writer.WriteLine(
                    string.Join('\t',
                        $"[{DateTime.Now.ToString(DateTimeFormat)}]",
                        logLevel == LogLevel.None
                            ? string.Empty
                            : $"[{logLevel}]",
                        string.Join(Environment.NewLine,
                            guildName,
                            text)));
            }
            _ = writerSemaphore.TryRelease();
            await Task.Delay(1);
        }

        private async Task SendAsync(string text, LogLevel logLevel)
        {
            try
            {
                await GenericWriteLineAsync(text, logLevel);
            }
            catch { }
        }

        public async Task SendAsync(string text) => await SendAsync(text, defaultLogLevel);

        public void Send(string text, LogLevel logLevel)
        {
            try
            {
                GenericWriteLineAsync(text, logLevel).Wait();
            }
            catch { }
        }

        public void Send(string text) => Send(text, defaultLogLevel);

        public void Dispose()
        {
            writerSemaphore.TryDispose();
        }
    }
}
