﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Console logging class
    /// </summary>
    public sealed class LogHandler(TextWriter writer,
                                   string guildName,
                                   LogLevel defaultLogLevel = LogLevel.None)
    {
        private static readonly Semaphore writerSemaphore = new(1, 1);

        private async Task GenericWriteLineAsync(string text, LogLevel logLevel)
        {
            if (string.IsNullOrWhiteSpace(text) || writer == null)
            {
                return;
            }
            if (!writerSemaphore.WaitOne(1000))
            {
                return;
            }

            await writer.WriteLineAsync(
                $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}]\t" +
                $"{(logLevel == LogLevel.None ? "" : $"[{logLevel}]")}\t" +
                $"{guildName}" +
                $"{Environment.NewLine}{text}");

            _ = writerSemaphore.Release(1);
        }

        public async Task SendAsync(string text, LogLevel logLevel)
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
    }
}
