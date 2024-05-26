using DSharpPlus.Entities;
using MyGreatestBot.Extensions;
using System;
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

        private void Send(DiscordMessageBuilder messageBuilder)
        {
            if (Channel is null)
            {
                return;
            }
            if (!messageSendSemaphore.TryWaitOne(messageDelay))
            {
                return;
            }

            if (!Channel.SendMessageAsync(messageBuilder).Wait(messageDelay))
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    "Cannot send message");

                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    messageBuilder.Content ?? "Cannot get message content");
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
            messageSendSemaphore.TryDispose();
        }
    }
}
