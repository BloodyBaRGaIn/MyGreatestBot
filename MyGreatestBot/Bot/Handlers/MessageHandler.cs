using DSharpPlus.Entities;
using MyGreatestBot.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MyGreatestBot.Bot.Handlers
{
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
                _ = await Channel.SendMessageAsync(embed);
            }
        }

        public async Task SendAsync(string message)
        {
            if (Channel != null)
            {
                _ = await Channel.SendMessageAsync(message);
            }
        }

        public void Send(DiscordEmbedBuilder embed)
        {
            if (SendAsync(embed).Wait(MessageDelay))
            {
                Task.Delay(MessageDelay).Wait();
            }
        }

        public void Send(Exception exception)
        {
            Send(exception.GetDiscordEmbed());
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
