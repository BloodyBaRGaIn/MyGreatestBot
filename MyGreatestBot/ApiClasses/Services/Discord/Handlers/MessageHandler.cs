using DSharpPlus;
using DSharpPlus.Entities;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Discord messages handler class
    /// </summary>
    public sealed class MessageHandler : IDisposable
    {
        private const int MaxRequestsPerSecond = 15;
        private static readonly int MinRequestDelay = (1000 / MaxRequestsPerSecond) + 1;

        [AllowNull] public DiscordChannel Channel { get; set; }

        private readonly Queue<DiscordMessageBuilder> messageQueue = new();
        private readonly CancellationTokenSource cts = new();
        private readonly Task task;

        private readonly string guildName;
        private readonly int messageDelay;

        private bool disposed;

        public MessageHandler(string guildName, int messageDelay)
        {
            this.guildName = guildName;
            this.messageDelay = messageDelay;

            task = Task.Run(MessageTask, cts.Token);
        }

        private void MessageTask()
        {
            Thread.CurrentThread.Name = $"{nameof(MessageTask)} \"{guildName}\"";

            while (true)
            {
                if (cts.IsCancellationRequested || disposed)
                {
                    break;
                }

                try
                {
                    Task.Delay(1).Wait();
                }
                catch
                {
                    return;
                }

                if (!messageQueue.TryDequeue(out DiscordMessageBuilder? builder))
                {
                    continue;
                }

                if (Channel is null)
                {
                    DiscordWrapper.CurrentDomainLogErrorHandler.Send("Message channel is null");

                    continue;
                }

                if (builder is null)
                {
                    DiscordWrapper.CurrentDomainLogErrorHandler.Send("Message is null");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(builder.Content)
                    && (builder.Embeds.Count == 0
                        || builder.Embeds.All(e =>
                        {
                            return string.IsNullOrWhiteSpace(e.Title)
                                && string.IsNullOrWhiteSpace(e.Description);
                        })))
                {
                    DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                        "Message corrupted");

                    continue;
                }

                bool result;

                try
                {
                    result = Channel.SendMessageAsync(builder).Wait(messageDelay);
                }
                catch
                {
                    result = false;
                }

                if (result)
                {
                    try
                    {
                        Task.Delay(MinRequestDelay).Wait();
                    }
                    catch { }
                    continue;
                }

                string? content = builder.Content;

                if (string.IsNullOrWhiteSpace(content))
                {
                    content = builder.Embeds.Count == 0
                        ? string.Empty
                        : string.Join(Environment.NewLine, builder.Embeds.Select(e =>
                        {
                            string? title = e.Title;
                            if (string.IsNullOrWhiteSpace(title))
                            {
                                title = "No title provided";
                            }
                            string? description = e.Description;
                            if (string.IsNullOrWhiteSpace(description))
                            {
                                description = "No description provided";
                            }
                            return $"{title} {description}";
                        }));
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    content = "Cannot get message content";
                }

                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    string.Join(Environment.NewLine,
                                "Cannot send message",
                                content));
            }
        }

        private void Send(DiscordMessageBuilder messageBuilder)
        {
            messageQueue.Enqueue(messageBuilder.SuppressNotifications());
        }

        private static DiscordMessageBuilder GetBuilder(string message)
        {
            return new DiscordMessageBuilder().WithContent(message);
        }

        private static DiscordMessageBuilder GetBuilder(DiscordEmbedBuilder embed)
        {
            return new DiscordMessageBuilder().AddEmbed(embed);
        }

        public void Send(DiscordEmbedBuilder embed)
        {
            if (embed is not null)
            {
                Send(GetBuilder(embed));
            }
        }

        public void Send(Exception exception)
        {
            if (exception is not null)
            {
                Send(exception.GetDiscordEmbed());
            }
        }

        public void Send(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Send(GetBuilder(message));
            }
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
            cts.Cancel();
            try
            {
                task.Wait();
            }
            catch { }
            finally
            {
                cts.Dispose();
            }
            try
            {
                task.Dispose();
            }
            catch { }
        }

        ~MessageHandler()
        {
            Dispose(false);
        }
    }
}
