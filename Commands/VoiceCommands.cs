using DicordNET.TrackClasses;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace DicordNET.Commands
{
    internal class VoiceCommands : BaseCommandModule
    {
        [Command("join")]
        [Aliases("j")]
        [Category("connection")]
        [Description("Join voice channel")]
        public async Task Join(CommandContext ctx)
        {
            StaticBotInstanceContainer.TextChannel = ctx.Channel;
            StaticBotInstanceContainer.VoiceNext = ctx.Client.GetVoiceNext();
            StaticBotInstanceContainer.VoiceConnection =
                StaticBotInstanceContainer.GetVoiceConnection(ctx.Guild);

            if (StaticBotInstanceContainer.VoiceConnection != null)
            {
                StaticBotInstanceContainer.Disconnect();
                await Join(ctx);
                return;
                //throw new InvalidOperationException("Already connected in this guild.");
            }

            StaticBotInstanceContainer.VoiceChannel = (ctx.Member?.VoiceState?.Channel)
                ?? throw new InvalidOperationException("You need to be in a voice channel.");

            StaticBotInstanceContainer.Connect();

            PlayerManager.Resume(ActionSource.Mute);
        }

        [Command("leave")]
        [Aliases("l")]
        [Category("connection")]
        [Description("Leave voice channel")]
        public async Task Leave(CommandContext ctx)
        {
            PlayerManager.Stop();

            StaticBotInstanceContainer.TextChannel = ctx.Channel;
            StaticBotInstanceContainer.VoiceNext = ctx.Client.GetVoiceNext();
            StaticBotInstanceContainer.VoiceConnection = StaticBotInstanceContainer.GetVoiceConnection(ctx.Guild);

            if (StaticBotInstanceContainer.VoiceConnection == null)
            {
                throw new InvalidOperationException("Not connected in this guild.");
            }

            StaticBotInstanceContainer.Disconnect();

            await Task.Delay(1);
        }

        [Command("play")]
        [Aliases("p")]
        [Category("player")]
        [Description("Add tracks")]
        public async Task Play(CommandContext ctx, [RemainingText] string query)
        {
            StaticBotInstanceContainer.TextChannel = ctx.Channel;
            StaticBotInstanceContainer.VoiceNext = ctx.Client.GetVoiceNext();
            StaticBotInstanceContainer.VoiceConnection = StaticBotInstanceContainer.GetVoiceConnection(ctx.Guild);

            if (StaticBotInstanceContainer.VoiceConnection == null)
            {
                await Join(ctx);
                StaticBotInstanceContainer.VoiceConnection = StaticBotInstanceContainer.GetVoiceConnection(ctx.Guild);

                if (StaticBotInstanceContainer.VoiceConnection == null)
                {
                    return;
                }
            }

            List<ITrackInfo> tracks = TrackManager.GetAll(query);

            if (!tracks.Any())
            {
                throw new InvalidOperationException("No results");
            }

            PlayerManager.EnqueueTracks(tracks);
        }

        [Command("return")]
        [Aliases("rt")]
        [Category("player")]
        [Description("Return track to queue")]
        public async Task ReturnCommand(CommandContext ctx)
        {
            StaticBotInstanceContainer.TextChannel = ctx.Channel;
            StaticBotInstanceContainer.VoiceNext = ctx.Client.GetVoiceNext();
            StaticBotInstanceContainer.VoiceConnection = StaticBotInstanceContainer.GetVoiceConnection(ctx.Guild);

            PlayerManager.ReturnCurrentTrackToQueue();

            await Task.Delay(1);
        }

        [Command("shuffle")]
        [Aliases("sh")]
        [Category("player")]
        [Description("Shuffle queue")]
        public async Task Shuffle(CommandContext ctx)
        {
            StaticBotInstanceContainer.TextChannel = ctx.Channel;

            PlayerManager.ShuffleQueue();

            await Task.Delay(1);
        }

        [Command("count")]
        [Aliases("cn")]
        [Category("player")]
        [Description("Get queue length")]
        public async Task GetCount(CommandContext ctx)
        {
            StaticBotInstanceContainer.TextChannel = ctx.Channel;

            PlayerManager.GetQueueLength();

            await Task.Delay(1);
        }

        [Command("track")]
        [Aliases("tr")]
        [Category("player")]
        [Description("Get current track")]
        public async Task GetTrackInfo(CommandContext ctx)
        {
            StaticBotInstanceContainer.TextChannel = ctx.Channel;

            PlayerManager.GetCurrentTrackInfo();

            await Task.Delay(1);
        }

        [Command("pause")]
        [Aliases("ps")]
        [Category("player")]
        [Description("Pause")]
        public async Task Pause(CommandContext ctx)
        {
            StaticBotInstanceContainer.TextChannel = ctx.Channel;

            PlayerManager.Pause();

            await Task.Delay(1);
        }

        [Command("resume")]
        [Aliases("rs")]
        [Category("player")]
        [Description("Resume")]
        public async Task Resume(CommandContext ctx)
        {
            StaticBotInstanceContainer.TextChannel = ctx.Channel;

            PlayerManager.Resume();

            await Task.Delay(1);
        }

        [Command("stop")]
        [Category("player")]
        [Description("Stop")]
        public async Task Stop(CommandContext ctx)
        {
            StaticBotInstanceContainer.TextChannel = ctx.Channel;

            try
            {
                await Leave(ctx);
            }
            catch { }

            await Task.Delay(1);
        }

        [Command("skip")]
        [Aliases("s")]
        [Category("player")]
        [Description("Skip")]
        public async Task Skip(CommandContext ctx)
        {
            StaticBotInstanceContainer.TextChannel = ctx.Channel;

            PlayerManager.Skip();

            await Task.Delay(1);
        }

        [Command("logout")]
        [Category("connection")]
        [Description("Logout")]
        public async Task LogoutCommand(CommandContext ctx)
        {
            _ = await ctx.Channel.SendMessageAsync(":wave:");
            var bot_client = StaticBotInstanceContainer.Client;

            if (bot_client != null)
            {
                await bot_client.UpdateStatusAsync(null, UserStatus.Offline);

                PlayerManager.Terminate();

                await bot_client.DisconnectAsync();
                bot_client.Dispose();
            }

            Environment.Exit(0);
        }
    }
}
