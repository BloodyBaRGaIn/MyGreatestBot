using DSharpPlus.Entities;
using MyGreatestBot.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Discord messages handler class
    /// </summary>
    public sealed class MessageHandler(int messageDelay)
    {
        [AllowNull]
        public DiscordChannel Channel { get; set; }

        private readonly Semaphore messageSendSemaphore = new(1, 1);

        private async Task SendAsync(DiscordMessageBuilder messageBuilder)
        {
            if (Channel is null)
            {
                return;
            }
            if (!messageSendSemaphore.WaitOne(messageDelay))
            {
                return;
            }

            _ = await Channel.SendMessageAsync(messageBuilder);
            _ = messageSendSemaphore.Release();
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
            SendAsync(GetBuilder(embed)).Wait();
        }

        public void Send(Exception exception)
        {
            Send(exception.GetDiscordEmbed());
        }

        public void Send(string message)
        {
            SendAsync(GetBuilder(message)).Wait();
        }
    }
}
