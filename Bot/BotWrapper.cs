using DicordNET.Commands;
using DicordNET.Player;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace DicordNET.Bot
{
    internal static class BotWrapper
    {
        private const int SEND_MESSAGE_WAIT_MS = 1000;

        internal static readonly DiscordBot BotInstance = new();

        internal static DiscordClient? Client => BotInstance.Client;
        internal static CommandsNextExtension? Commands => BotInstance.Commands;
        internal static IServiceProvider ServiceProvider => BotInstance.ServiceProvider;

        internal static VoiceNextExtension? VoiceNext;
        internal static VoiceNextConnection? VoiceConnection;
        internal static DiscordChannel? TextChannel;
        internal static DiscordChannel? VoiceChannel;
        internal static VoiceTransmitSink? TransmitSink;

        internal static void Run()
        {
            BotInstance.RunAsync().GetAwaiter().GetResult();
        }

        internal static VoiceNextConnection? GetVoiceConnection(DiscordGuild? guild)
        {
            try
            {
                return VoiceNext?.GetConnection(guild);
            }
            catch
            {
                return VoiceConnection;
            }
        }

        internal static async Task SendMessageAsync(DiscordEmbedBuilder embed)
        {
            if (TextChannel != null)
            {
                _ = await TextChannel.SendMessageAsync(embed);
            }
        }

        internal static async Task SendMessageAsync(string message)
        {
            if (TextChannel != null)
            {
                _ = await TextChannel.SendMessageAsync(message);
            }
        }

        internal static void SendMessage(DiscordEmbedBuilder embed)
        {
            _ = SendMessageAsync(embed).Wait(SEND_MESSAGE_WAIT_MS);
        }

        internal static void SendMessage(string message)
        {
            _ = SendMessageAsync(message).Wait(SEND_MESSAGE_WAIT_MS);
        }

        internal static void SendSpeaking(bool speaking)
        {
            try
            {
                _ = VoiceConnection?.SendSpeakingAsync(speaking).Wait(100);
            }
            catch { }
        }

        internal static void Connect()
        {
            _ = (VoiceNext?.ConnectAsync(VoiceChannel).Wait(1000));
        }

        internal static void Disconnect()
        {
            try
            {
                VoiceConnection?.Disconnect();
                VoiceConnection = null;
            }
            catch { }
        }

        internal static async Task Join(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            TextChannel = ctx.Channel;
            VoiceNext = ctx.Client.GetVoiceNext();
            VoiceConnection = GetVoiceConnection(ctx.Guild);

            if (VoiceConnection != null)
            {
                Disconnect();
                await Join(ctx);
                return;
                //throw new InvalidOperationException("Already connected in this guild.");
            }

            VoiceChannel = (ctx.Member?.VoiceState?.Channel)
                ?? throw new InvalidOperationException("You need to be in a voice channel.");

            Connect();

            await Task.Run(() => PlayerManager.Resume(CommandActionSource.Mute));

            await Task.Delay(1);
        }

        public static async Task Leave(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            PlayerManager.Stop(CommandActionSource.Mute);

            TextChannel = ctx.Channel;
            VoiceNext = ctx.Client.GetVoiceNext();
            VoiceConnection = GetVoiceConnection(ctx.Guild);

            //if (VoiceConnection == null)
            //{
            //    throw new InvalidOperationException("Not connected in this guild.");
            //}

            Disconnect();

            await Task.Delay(1);
        }
    }
}
