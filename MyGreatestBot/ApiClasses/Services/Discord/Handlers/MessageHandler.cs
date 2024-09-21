using DSharpPlus.Entities;
using MyGreatestBot.Extensions;
using System;
using System.Linq;
using System.Threading;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Discord messages handler class
    /// </summary>
    public sealed class MessageHandler(int messageDelay) : IDisposable
    {
        [AllowNull] public DiscordChannel Channel { get; set; }

        private readonly Semaphore messageSendSemaphore = new(1, 1);

        private bool disposed;

        private void Send(DiscordMessageBuilder messageBuilder)
        {
            if (Channel is null)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send("Message channel is null");

                return;
            }

            if (messageBuilder is null)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send("Message is null");

                return;
            }

            if (string.IsNullOrWhiteSpace(messageBuilder.Content)
                && (messageBuilder.Embeds.Count == 0
                    || messageBuilder.Embeds.All(e =>
                    {
                        return string.IsNullOrWhiteSpace(e.Title)
                            && string.IsNullOrWhiteSpace(e.Description);
                    })))
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    "Message corrupted");

                return;
            }

            if (!messageSendSemaphore.TryWaitOne(messageDelay))
            {
                return;
            }

            if (!Channel.SendMessageAsync(messageBuilder).Wait(messageDelay))
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send("Cannot send message");

                string? content = messageBuilder.Content;

                if (string.IsNullOrWhiteSpace(content))
                {
                    content = messageBuilder.Embeds.Count == 0
                        ? string.Empty
                        : string.Join(Environment.NewLine, messageBuilder.Embeds.Select(e =>
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

                DiscordWrapper.CurrentDomainLogErrorHandler.Send(content);
            }
            _ = messageSendSemaphore.TryRelease();
        }

        private static DiscordMessageBuilder GetBuilder(string message)
        {
            return new DiscordMessageBuilder().WithContent(message).SuppressNotifications();
        }

        private static DiscordMessageBuilder GetBuilder(DiscordEmbedBuilder embed)
        {
            return new DiscordMessageBuilder().AddEmbed(embed).SuppressNotifications();
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
            messageSendSemaphore.TryDispose();
        }

        ~MessageHandler()
        {
            Dispose(false);
        }
    }
}
