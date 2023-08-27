using DicordNET.ApiClasses;
using DicordNET.Bot;
using DicordNET.Player;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace DicordNET.Commands
{
    [Category(CommandStrings.PlayerCategoryName)]
    [SupportedOSPlatform("windows")]
    internal class PlayerCommands : BaseCommandModule
    {
        private static async Task<IEnumerable<ITrackInfo>> GenericPlay(CommandContext ctx, ConnectionHandler handler, string? query)
        {
            handler.TextChannel = ctx.Channel;
            handler.VoiceConnection = handler.GetVoiceConnection();

            if (handler.VoiceConnection == null)
            {
                await ConnectionHandler.Join(ctx);
                handler.VoiceConnection = handler.GetVoiceConnection();

                if (handler.VoiceConnection == null)
                {
                    throw new InvalidOperationException("Cannot establish a voice connection");
                }
            }

            return ApiConfig.GetAll(query);
        }

        [Command("play")]
        [Aliases("p")]
        [Description("Add tracks")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Play(CommandContext ctx, [RemainingText, Description("URL")] string query)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            IEnumerable<ITrackInfo> tracks = await GenericPlay(ctx, handler, query);

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks));
        }

        [Command("tms")]
        [Aliases("t")]
        [Description("Place query result to the head")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task TmsCommand(CommandContext ctx, [RemainingText, Description("URL")] string query)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            IEnumerable<ITrackInfo> tracks = await GenericPlay(ctx, handler, query);

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks, CommandActionSource.External));
        }

        [Command("seek")]
        [Aliases("sk")]
        [Description("Seek current track")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task SeekCommand(CommandContext ctx, [Description("Timespan in format HH:MM:SS")] string timespan)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;
            handler.VoiceConnection = handler.GetVoiceConnection();

            if (!TimeSpan.TryParse(timespan, out TimeSpan result))
            {
                throw new InvalidCastException("Invalid argument format");
            }

            await Task.Run(() => handler.PlayerInstance.RequestSeek(result));

            await Task.Delay(1);
        }

        [Command("return")]
        [Aliases("rt")]
        [Description("Return track to queue")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task ReturnCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;
            handler.VoiceConnection = handler.GetVoiceConnection();

            await Task.Run(handler.PlayerInstance.ReturnCurrentTrackToQueue);

            await Task.Delay(1);
        }

        [Command("shuffle")]
        [Aliases("sh")]
        [Description("Shuffle queue")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Shuffle(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(handler.PlayerInstance.ShuffleQueue);

            await Task.Delay(1);
        }

        [Command("count")]
        [Aliases("cn")]
        [Description("Get queue length")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task GetCount(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(handler.PlayerInstance.GetQueueLength);

            await Task.Delay(1);
        }

        [Command("track")]
        [Aliases("tr")]
        [Description("Get current track")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task GetTrackInfo(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(handler.PlayerInstance.GetCurrentTrackInfo);

            await Task.Delay(1);
        }

        [Command("next")]
        [Aliases("ntr")]
        [Description("Get next track")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task GetNextTrackInfo(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(handler.PlayerInstance.GetNextTrackInfo);

            await Task.Delay(1);
        }

        [Command("pause")]
        [Aliases("ps")]
        [Description("Pause")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Pause(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Pause());

            await Task.Delay(1);
        }

        [Command("resume")]
        [Aliases("rs")]
        [Description("Resume")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Resume(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Resume());

            await Task.Delay(1);
        }

        [Command("stop")]
        [Aliases("st")]
        [Description("Stop")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Stop(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            try
            {
                await ConnectionHandler.Leave(ctx);
            }
            catch { }

            await Task.Delay(1);
        }

        [Command("skip")]
        [Aliases("s")]
        [Description("Skip")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Skip(
            CommandContext ctx,
            [AllowNull, Description("Number of tracks to skip")] int number = 1)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            if (number < 1)
            {
                throw new ArgumentException("Number must be positive");
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Skip(number - 1));

            await Task.Delay(1);
        }

        [Command("ignore")]
        [Aliases("it", "i")]
        [Description("Ignore current track")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Ignore(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.IgnoreTrack());

            await Task.Delay(1);
        }

        [Command("ignoreartist")]
        [Aliases("ia")]
        [Description("Ignore current track artist")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task IgnoreArtist(
            CommandContext ctx,
            [AllowNull, Description("Artist zero-based index")] int artist_index = -1)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.IgnoreArtist(artist_index));

            await Task.Delay(1);
        }
    }
}
