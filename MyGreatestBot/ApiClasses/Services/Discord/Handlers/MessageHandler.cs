using DSharpPlus.Entities;
using MyGreatestBot.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Discord.Handlers
{
    /// <summary>
    /// Discord messages handler class
    /// </summary>
    public sealed class MessageHandler
    {
        [AllowNull]
        public DiscordChannel Channel { get; set; }

        private readonly int MessageDelay;

        public MessageHandler(int messageDelay)
        {
            MessageDelay = messageDelay;
        }

        public async Task SendAsync(DiscordEmbedBuilder embed)
        {
            if (Channel != null)
            {
                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder().AddEmbed(embed).SuppressNotifications();
                _ = await Channel.SendMessageAsync(messageBuilder);
            }
        }

        public async Task SendAsync(string message)
        {
            if (Channel != null)
            {
                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder().WithContent(message).SuppressNotifications();
                _ = await Channel.SendMessageAsync(messageBuilder);
            }
        }

        public void Send(DiscordEmbedBuilder embed)
        {
            if (SendAsync(embed).Wait(MessageDelay))
            {
                Task.Delay(MessageDelay).Wait();
            }
        }

        public void Send(Exception exception, bool isSuccess = false)
        {
            Send(exception.GetDiscordEmbed(isSuccess));
        }

        public void Send(string message)
        {
            if (SendAsync(message).Wait(MessageDelay))
            {
                Task.Delay(MessageDelay).Wait();
            }
        }
    }
}
