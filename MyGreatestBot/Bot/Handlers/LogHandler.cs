using System;
using System.IO;
using System.Threading.Tasks;

namespace MyGreatestBot.Bot.Handlers
{
    public sealed class LogHandler
    {
        private readonly TextWriter _writer;
        private readonly string _guildName;
        public LogHandler(TextWriter writer, string guildName)
        {
            _writer = writer;
            _guildName = guildName;
        }

        private async Task GenericWriteLineAsync(string text)
        {
            if (!string.IsNullOrWhiteSpace(text) && _writer != null)
            {
                await _writer.WriteLineAsync($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}]\t{_guildName}{Environment.NewLine}{text}");
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
