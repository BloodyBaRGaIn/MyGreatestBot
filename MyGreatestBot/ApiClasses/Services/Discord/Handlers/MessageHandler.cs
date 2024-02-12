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
    public sealed class MessageHandler(int messageDelay)
    {
        [AllowNull]
        public DiscordChannel Channel { get; set; }

        public async Task SendAsync(DiscordEmbedBuilder embed)
        {
            if (Channel is null)
            {
                return;
            }

            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                .AddEmbed(embed)
                .SuppressNotifications();

            _ = await Channel.SendMessageAsync(messageBuilder);
        }

        public async Task SendAsync(string message)
        {
            if (Channel is null)
            {
                return;
            }

            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                .WithContent(message)
                .SuppressNotifications();

            _ = await Channel.SendMessageAsync(messageBuilder);
        }

        public void Send(DiscordEmbedBuilder embed)
        {
            if (SendAsync(embed).Wait(messageDelay))
            {
                Task.Delay(messageDelay).Wait();
            }
        }

        public void Send(Exception exception)
        {
            Send(exception.GetDiscordEmbed());
        }

        public void Send(string message)
        {
            if (SendAsync(message).Wait(messageDelay))
            {
                Task.Delay(messageDelay).Wait();
            }
        }
    }
}
