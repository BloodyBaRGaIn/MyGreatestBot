﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Bot;
using MyGreatestBot.Extensions;
using MyGreatestBot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
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

            IEnumerable<ITrackInfo> tracks = await GenericPlay(ctx, handler, query);

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks));
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

            IEnumerable<ITrackInfo> tracks = await GenericPlay(ctx, handler, query);

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks.Shuffle()));
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

            IEnumerable<ITrackInfo> tracks = await GenericPlay(ctx, handler, query);

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks, CommandActionSource.External));
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
            handler.VoiceConnection = handler.GetVoiceConnection();

            TimeSpan time = TimeSpan.Zero;

            InvalidCastException invalidCastException = new("Invalid argument format");

            IEnumerable<string> split = timespan.Split(':').Reverse();

            int count = split.Count();

            if (count is < 2 or > 3)
            {
                throw invalidCastException;
            }

            string? seconds = split.ElementAtOrDefault(0);
            string? minutes = split.ElementAtOrDefault(1);
            string? hours = split.ElementAtOrDefault(2);

            if (seconds != null)
            {
                if (!uint.TryParse(seconds, out uint value) || value > 59)
                {
                    throw invalidCastException;
                }
                time += TimeSpan.FromSeconds(value);
            }

            if (minutes != null)
            {
                if (!uint.TryParse(minutes, out uint value) || value > 59)
                {
                    throw invalidCastException;
                }
                time += TimeSpan.FromMinutes(value);
            }

            if (hours != null && count == 3)
            {
                if (!uint.TryParse(hours, out uint value))
                {
                    throw invalidCastException;
                }
                time += TimeSpan.FromHours(value);
            }

            await Task.Run(() => handler.PlayerInstance.RequestSeek(time));

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
        public async Task ShuffleCommand(CommandContext ctx)
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

            await Task.Run(() => handler.PlayerInstance.Pause());

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

            await Task.Run(() => handler.PlayerInstance.Resume());

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

            try
            {
                await Task.Run(() => handler.PlayerInstance.Stop());
            }
            catch { }

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
        public async Task IgnoreCommand(CommandContext ctx)
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

            await Task.Run(() => handler.PlayerInstance.IgnoreArtist(artist_index));

            await Task.Delay(1);
        }
    }
}