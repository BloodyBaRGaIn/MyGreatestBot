using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Bot.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
{
    [Category(CommandStrings.PlayerCategoryName)]
    [SupportedOSPlatform("windows")]
    internal class PlayerCommands : BaseCommandModule
    {
        private static async Task<IEnumerable<ITrackInfo>> GetTracks(CommandContext ctx, ConnectionHandler handler, string query)
        {
            handler.TextChannel = ctx.Channel;
            handler.Voice.UpdateVoiceConnection();

            if (handler.VoiceConnection == null)
            {
                await handler.Join(ctx);
                await handler.Voice.WaitForConnectionAsync();
                handler.Update(ctx.Guild);
            }

            return ApiManager.GetAll(query);
        }

        [Command("play")]
        [Aliases("p")]
        [Description("Add tracks")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task PlayCommand(
            CommandContext ctx,
            [RemainingText, Description("URL")] string query)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            IEnumerable<ITrackInfo> tracks = await GetTracks(ctx, handler, query);

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks, CommandActionSource.Command));
        }

        [Command("playshuffled")]
        [Aliases("psh")]
        [Description("Add shuffled tracks")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task PlayShuffledCommand(
            CommandContext ctx,
            [RemainingText, Description("URL")] string query)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            IEnumerable<ITrackInfo> tracks = await GetTracks(ctx, handler, query);

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks, CommandActionSource.Command | CommandActionSource.PlayerShuffle));
        }

        [Command("tms")]
        [Aliases("t")]
        [Description("Place query result to the head")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task TmsCommand(
            CommandContext ctx,
            [RemainingText, Description("URL")] string query)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            IEnumerable<ITrackInfo> tracks = await GetTracks(ctx, handler, query);

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks, CommandActionSource.Command | CommandActionSource.PlayerToHead));
        }

        [Command("seek")]
        [Aliases("sk")]
        [Description("Seek current track")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task SeekCommand(
            CommandContext ctx,
            [Description("Timespan in format HH:MM:SS or MM:SS")] string timespan)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;
            handler.Voice.UpdateVoiceConnection();

            string[] formats = new[]
            {
                "HH:mm:ss",
                "mm:ss"
            };

            TimeSpan time = TimeSpan.MinValue;

            foreach (string format in formats)
            {
                try
                {
                    time = DateTime.ParseExact(timespan, format, CultureInfo.InvariantCulture).TimeOfDay;
                    break;
                }
                catch { }
            }

            if (time == TimeSpan.MinValue)
            {
                throw new SeekException("Wrong format");
            }

            await Task.Run(() => handler.PlayerInstance.RequestSeek(time, CommandActionSource.Command));

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
            handler.Voice.UpdateVoiceConnection();

            await Task.Run(() => handler.PlayerInstance.ReturnCurrentTrackToQueue(CommandActionSource.Command));

            await Task.Delay(1);
        }

        [Command("shuffle")]
        [Aliases("sh")]
        [Description("Shuffle queue")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task ShuffleCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.ShuffleQueue(CommandActionSource.Command));

            await Task.Delay(1);
        }

        [Command("count")]
        [Aliases("cnt", "cn")]
        [Description("Get queue length")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task CountCommand(CommandContext ctx)
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

        [Command("currenttrack")]
        [Aliases("track", "tr")]
        [Description("Get current track")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task TrackInfoCommand(CommandContext ctx)
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

        [Command("nexttrack")]
        [Aliases("next", "ntr", "nex")]
        [Description("Get next track")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task NextTrackInfoCommand(CommandContext ctx)
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
        public async Task PauseCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Pause(CommandActionSource.Command));

            await Task.Delay(1);
        }

        [Command("resume")]
        [Aliases("rs")]
        [Description("Resume")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task ResumeCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Resume(CommandActionSource.Command));

            await Task.Delay(1);
        }

        [Command("clear")]
        [Aliases("clr", "cl", "c")]
        [Description("Clear the queue")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task ClearCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Clear(CommandActionSource.Command));

            await Task.Delay(1);
        }

        [Command("stop")]
        [Aliases("st")]
        [Description("Stop")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task StopCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Stop(CommandActionSource.Command));

            await Task.Delay(1);
        }

        [Command("skip")]
        [Aliases("s")]
        [Description("Skip")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task SkipCommand(
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
                throw new SkipException("Number must be positive");
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Skip(number - 1, CommandActionSource.Command));

            await Task.Delay(1);
        }

        [Command("ignore")]
        [Aliases("it", "i")]
        [Description("Ignore current track")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task IgnoreCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.IgnoreTrack(CommandActionSource.Command));

            await Task.Delay(1);
        }

        [Command("ignoreartist")]
        [Aliases("ia")]
        [Description("Ignore current track artist")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task IgnoreArtistCommand(
            CommandContext ctx,
            [AllowNull, Description("Artist zero-based index")] int artist_index = -1)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.IgnoreArtist(artist_index, CommandActionSource.Command));

            await Task.Delay(1);
        }
    }
}
