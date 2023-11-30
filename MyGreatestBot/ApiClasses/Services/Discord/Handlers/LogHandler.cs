using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Console logging class
    /// </summary>
    public sealed class LogHandler
    {
        private readonly TextWriter _writer;
        private readonly string _guildName;

        private static Semaphore writerSemaphore = new(1, 1);

        public LogHandler(TextWriter writer, string guildName)
        {
            _writer = writer;
            _guildName = guildName;
        }

        private async Task GenericWriteLineAsync(string text)
        {
            if (!string.IsNullOrWhiteSpace(text) && _writer != null)
            {
                if (!writerSemaphore.WaitOne(1000))
                {
                    return;
                }
                await _writer.WriteLineAsync($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}]\t{_guildName}{Environment.NewLine}{text}");
                _ = writerSemaphore.Release(1);
            }
        }

        public async Task SendAsync(string text)
        {
            await GenericWriteLineAsync(text);
        }

        public void Send(string text)
        {
            GenericWriteLineAsync(text).Wait();
        }
    }
}
