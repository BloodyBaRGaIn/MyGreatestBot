using DicordNET.Bot;
using DicordNET.Player;
using DicordNET.TrackClasses;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;

namespace DicordNET.Commands
{
    [Category(CommandStrings.PlayerCategoryName)]
    internal class PlayerCommands : BaseCommandModule
    {
        [Command("play")]
        [Aliases("p")]
        [Description("Add tracks")]
        public async Task Play(CommandContext ctx, [RemainingText] string? query)
        {
            BotWrapper.TextChannel = ctx.Channel;
            BotWrapper.VoiceNext = ctx.Client.GetVoiceNext();
            BotWrapper.VoiceConnection = BotWrapper.GetVoiceConnection(ctx.Guild);

            if (BotWrapper.VoiceConnection == null)
            {
                await BotWrapper.Join(ctx);
                BotWrapper.VoiceConnection = BotWrapper.GetVoiceConnection(ctx.Guild);

                if (BotWrapper.VoiceConnection == null)
                {
                    return;
                }
            }

            List<ITrackInfo> tracks = TrackManager.GetAll(query);

            if (!tracks.Any())
            {
                throw new InvalidOperationException("No results");
            }

            PlayerManager.Enqueue(tracks);
        }

        [Command("tms")]
        [Description("Place query result to queue head")]
        public async Task TmsCommand(CommandContext ctx, [RemainingText] string? query)
        {
            BotWrapper.TextChannel = ctx.Channel;
            BotWrapper.VoiceNext = ctx.Client.GetVoiceNext();
            BotWrapper.VoiceConnection = BotWrapper.GetVoiceConnection(ctx.Guild);

            if (BotWrapper.VoiceConnection == null)
            {
                await BotWrapper.Join(ctx);
                BotWrapper.VoiceConnection = BotWrapper.GetVoiceConnection(ctx.Guild);

                if (BotWrapper.VoiceConnection == null)
                {
                    return;
                }
            }

            List<ITrackInfo> tracks = TrackManager.GetAll(query);

            if (!tracks.Any())
            {
                throw new InvalidOperationException("No results");
            }

            PlayerManager.Enqueue(tracks, CommandActionSource.External);
        }

        [Command("seek")]
        [Description("Seek current track")]
        public async Task SeekCommand(CommandContext ctx, string span_str)
        {
            BotWrapper.TextChannel = ctx.Channel;
            BotWrapper.VoiceNext = ctx.Client.GetVoiceNext();
            BotWrapper.VoiceConnection = BotWrapper.GetVoiceConnection(ctx.Guild);

            if (!TimeSpan.TryParse(span_str, out TimeSpan result))
            {
                throw new InvalidCastException();
            }

            PlayerManager.RequestSeek(result);

            await Task.Delay(1);
        }

        [Command("return")]
        [Aliases("rt")]
        [Description("Return track to queue")]
        public async Task ReturnCommand(CommandContext ctx)
        {
            BotWrapper.TextChannel = ctx.Channel;
            BotWrapper.VoiceNext = ctx.Client.GetVoiceNext();
            BotWrapper.VoiceConnection = BotWrapper.GetVoiceConnection(ctx.Guild);

            PlayerManager.ReturnCurrentTrackToQueue();

            await Task.Delay(1);
        }

        [Command("shuffle")]
        [Aliases("sh")]
        [Description("Shuffle queue")]
        public async Task Shuffle(CommandContext ctx)
        {
            BotWrapper.TextChannel = ctx.Channel;

            PlayerManager.ShuffleQueue();

            await Task.Delay(1);
        }

        [Command("count")]
        [Aliases("cn")]
        [Description("Get queue length")]
        public async Task GetCount(CommandContext ctx)
        {
            BotWrapper.TextChannel = ctx.Channel;

            PlayerManager.GetQueueLength();

            await Task.Delay(1);
        }

        [Command("track")]
        [Aliases("tr")]
        [Description("Get current track")]
        public async Task GetTrackInfo(CommandContext ctx)
        {
            BotWrapper.TextChannel = ctx.Channel;

            PlayerManager.GetCurrentTrackInfo();

            await Task.Delay(1);
        }

        [Command("pause")]
        [Aliases("ps")]
        [Description("Pause")]
        public async Task Pause(CommandContext ctx)
        {
            BotWrapper.TextChannel = ctx.Channel;

            PlayerManager.Pause();

            await Task.Delay(1);
        }

        [Command("resume")]
        [Aliases("rs")]
        [Description("Resume")]
        public async Task Resume(CommandContext ctx)
        {
            BotWrapper.TextChannel = ctx.Channel;

            PlayerManager.Resume();

            await Task.Delay(1);
        }

        [Command("stop")]
        [Aliases("st")]
        [Description("Stop")]
        public async Task Stop(CommandContext ctx)
        {
            BotWrapper.TextChannel = ctx.Channel;

            try
            {
                await BotWrapper.Leave(ctx);
            }
            catch { }

            await Task.Delay(1);
        }

        [Command("skip")]
        [Aliases("s")]
        [Description("Skip")]
        public async Task Skip(CommandContext ctx)
        {
            BotWrapper.TextChannel = ctx.Channel;

            PlayerManager.Skip();

            await Task.Delay(1);
        }
    }
}
