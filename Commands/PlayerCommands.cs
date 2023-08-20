using DicordNET.ApiClasses;
using DicordNET.Bot;
using DicordNET.Player;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using System.Diagnostics.CodeAnalysis;

namespace DicordNET.Commands
{
    [Category(CommandStrings.PlayerCategoryName)]
    internal class PlayerCommands : BaseCommandModule
    {
        private static async Task<IEnumerable<ITrackInfo>> GenericPlay(CommandContext ctx, string? query)
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
                    return Enumerable.Empty<ITrackInfo>();
                }
            }

            List<ITrackInfo> tracks = ApiConfig.GetAll(query);

            if (!tracks.Any())
            {
                throw new InvalidOperationException("No results");
            }

            return tracks;
        }

        [Command("play")]
        [Aliases("p")]
        [Description("Add tracks")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Play(CommandContext ctx, [RemainingText, Description("URL")] string query)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            IEnumerable<ITrackInfo> tracks = await GenericPlay(ctx, query);

            await Task.Run(() => PlayerManager.Enqueue(tracks));
        }

        [Command("tms")]
        [Aliases("t")]
        [Description("Place query result to the head")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task TmsCommand(CommandContext ctx, [RemainingText, Description("URL")] string query)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            IEnumerable<ITrackInfo> tracks = await GenericPlay(ctx, query);

            await Task.Run(() => PlayerManager.Enqueue(tracks, CommandActionSource.External));
        }

        [Command("seek")]
        [Aliases("sk")]
        [Description("Seek current track")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task SeekCommand(CommandContext ctx, [Description("Timespan in format HH:MM:SS")] string timespan)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            BotWrapper.TextChannel = ctx.Channel;
            BotWrapper.VoiceNext = ctx.Client.GetVoiceNext();
            BotWrapper.VoiceConnection = BotWrapper.GetVoiceConnection(ctx.Guild);

            if (!TimeSpan.TryParse(timespan, out TimeSpan result))
            {
                throw new InvalidCastException("Invalid argument format");
            }

            await Task.Run(() => PlayerManager.RequestSeek(result));

            await Task.Delay(1);
        }

        [Command("return")]
        [Aliases("rt")]
        [Description("Return track to queue")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task ReturnCommand(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            BotWrapper.TextChannel = ctx.Channel;
            BotWrapper.VoiceNext = ctx.Client.GetVoiceNext();
            BotWrapper.VoiceConnection = BotWrapper.GetVoiceConnection(ctx.Guild);

            await Task.Run(PlayerManager.ReturnCurrentTrackToQueue);

            await Task.Delay(1);
        }

        [Command("shuffle")]
        [Aliases("sh")]
        [Description("Shuffle queue")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Shuffle(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            BotWrapper.TextChannel = ctx.Channel;

            await Task.Run(PlayerManager.ShuffleQueue);

            await Task.Delay(1);
        }

        [Command("count")]
        [Aliases("cn")]
        [Description("Get queue length")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task GetCount(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            BotWrapper.TextChannel = ctx.Channel;

            await Task.Run(PlayerManager.GetQueueLength);

            await Task.Delay(1);
        }

        [Command("track")]
        [Aliases("tr")]
        [Description("Get current track")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task GetTrackInfo(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            BotWrapper.TextChannel = ctx.Channel;

            await Task.Run(PlayerManager.GetCurrentTrackInfo);

            await Task.Delay(1);
        }

        [Command("pause")]
        [Aliases("ps")]
        [Description("Pause")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Pause(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            BotWrapper.TextChannel = ctx.Channel;

            await Task.Run(() => PlayerManager.Pause());

            await Task.Delay(1);
        }

        [Command("resume")]
        [Aliases("rs")]
        [Description("Resume")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Resume(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            BotWrapper.TextChannel = ctx.Channel;

            await Task.Run(() => PlayerManager.Resume());

            await Task.Delay(1);
        }

        [Command("stop")]
        [Aliases("st")]
        [Description("Stop")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Stop(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }

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
        [SuppressMessage("Performance", "CA1822")]
        public async Task Skip(CommandContext ctx, [AllowNull, Description("Number of tracks to skip")] int number = 1)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            if (number < 1)
            {
                throw new ArgumentException("Number must be positive");
            }

            BotWrapper.TextChannel = ctx.Channel;

            await Task.Run(() => PlayerManager.Skip(number - 1));

            await Task.Delay(1);
        }

        [Command("ignore")]
        [Aliases("i")]
        [Description("Ignore current track")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Ignore(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            BotWrapper.TextChannel = ctx.Channel;

            await Task.Run(() => PlayerManager.Ignore());

            await Task.Delay(1);
        }
    }
}
