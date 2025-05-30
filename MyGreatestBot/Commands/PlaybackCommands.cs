﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
{
    [Category(CommandStrings.PlaybackCategoryName)]
    internal class PlaybackCommands : BaseCommandModule
    {
        [Command("pause"), Aliases("ps")]
        [Description("Pause playback")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task PauseCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Pause(CommandActionSource.Command));
        }

        [Command("resume"), Aliases("rs")]
        [Description("Resume playback")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task ResumeCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Resume(CommandActionSource.Command));
        }

        [Command("stop"), Aliases("st")]
        [Description("Stop playback")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task StopCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Stop(CommandActionSource.Command));
        }

        [Command("skip"), Aliases("s")]
        [Description("Skip current track")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
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
                throw new SkipCommandException("Number must be positive");
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Skip(number - 1, CommandActionSource.Command));
        }

        [Command("count"), Aliases("cnt", "cn")]
        [Description("Get the number of tracks in the queue")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task CountCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(handler.PlayerInstance.GetQueueLength);
        }

        [Command("clear"), Aliases("clr", "cl", "c")]
        [Description("Clear track queue")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task ClearCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Clear(CommandActionSource.Command));
        }

        [Command("shuffle"), Aliases("sh")]
        [Description("Shuffle queue")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task ShuffleCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.ShuffleQueue(CommandActionSource.Command));
        }

        [Command("unique"), Aliases("uniq", "u")]
        [Description("Removes all but unique tracks from the queue")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task UniqueCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.GetUniqueTracks(CommandActionSource.Command));
        }

        [Command("rewind")]
        [Aliases("seek", "rw", "sk")]
        [Description("Rewind audio stream")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task RewindCommand(
            CommandContext ctx,
            [Description(
            "Timespan in " +
            $"{TimeSpanRegexProvider.HoursMinutesSecondsFormat} or " +
            $"{TimeSpanRegexProvider.MinutesSecondsFormat} formats")] string timespan)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;
            handler.Voice.UpdateVoiceConnection();

            TimeSpan time = TimeSpanRegexProvider.GetTimeSpan(timespan);

            if (time == TimeSpan.MinValue)
            {
                throw new RewindCommandException("Wrong format");
            }

            await Task.Run(() => handler.PlayerInstance.RequestRewind(time, CommandActionSource.Command));
        }

        [Command("return"), Aliases("rt")]
        [Description("Return the track to the queue")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
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
        }

        [Command("currenttrack"), Aliases("track", "tr")]
        [Description("Get information about the current track")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task TrackInfoCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(handler.PlayerInstance.GetCurrentTrackInfo);
        }

        [Command("nexttrack"), Aliases("next", "ntr")]
        [Description("Get information about the next track")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task NextTrackInfoCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(handler.PlayerInstance.GetNextTrackInfo);
        }
    }
}
