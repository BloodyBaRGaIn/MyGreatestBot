global using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Message logging class
    /// </summary>
    public sealed class LogHandler : IDisposable
    {
        public const string DateTimeFormat = "dd.MM.yyyy HH:mm:ss.fff";

        private static readonly Semaphore consoleSemaphore = new(1, 1);
        private static readonly Dictionary<TextWriter, Semaphore> semaphoreDictionary = new()
        {
            [Console.Out] = consoleSemaphore,
            [Console.Error] = consoleSemaphore
        };

        private readonly TextWriter writer;
        private readonly string guildName;
        private readonly int logDelay;
        private readonly LogLevel defaultLogLevel;

        private bool disposed;

        public LogHandler(TextWriter writer,
                          string guildName,
                          int logDelay,
                          LogLevel defaultLogLevel = LogLevel.None)
        {
            this.writer = writer;
            this.guildName = guildName;
            this.logDelay = logDelay;
            this.defaultLogLevel = defaultLogLevel;

            if (!semaphoreDictionary.ContainsKey(writer))
            {
                semaphoreDictionary.Add(writer, new(1, 1));
            }
        }

        private async Task GenericWriteLineAsync(string text, LogLevel logLevel)
        {
            if (string.IsNullOrWhiteSpace(text) || writer == null)
            {
                return;
            }
            Semaphore semaphore = semaphoreDictionary[writer];
            bool ready;
            try
            {
                ready = semaphore.WaitOne(logDelay);
            }
            catch
            {
                ready = true;
            }
            if (!ready)
            {
                return;
            }
            lock (writer)
            {
                string output = $"[{DateTime.Now.ToString(DateTimeFormat)}]\t";
                if (logLevel != LogLevel.None)
                {
                    output += $"[{logLevel}] ";
                }
                output += $"{guildName}{Environment.NewLine}{text}";
                writer.WriteLine(output);
            }
            _ = semaphore.TryRelease();
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

        public async Task SendAsync(string text)
        {
            await SendAsync(text, defaultLogLevel);
        }

        public void Send(string text, LogLevel logLevel)
        {
            try
            {
                GenericWriteLineAsync(text, logLevel).Wait();
            }
            catch { }
        }

        public void Send(string text)
        {
            Send(text, defaultLogLevel);
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
            Semaphore semaphore = semaphoreDictionary[writer];
            if (disposing || semaphore != consoleSemaphore)
            {
                semaphore.TryDispose();
            }
        }

        ~LogHandler()
        {
            Dispose(false);
        }
    }
}
