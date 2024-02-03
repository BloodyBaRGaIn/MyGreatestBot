using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Console logging class
    /// </summary>
    public sealed class LogHandler(TextWriter writer, string guildName)
    {
        private static readonly Semaphore writerSemaphore = new(1, 1);

        private async Task GenericWriteLineAsync(string text)
        {
            if (!string.IsNullOrWhiteSpace(text) && writer != null)
            {
                if (!writerSemaphore.WaitOne(1000))
                {
                    return;
                }
                await writer.WriteLineAsync($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}]\t{guildName}{Environment.NewLine}{text}");
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
