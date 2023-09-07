using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
{
    [Category(CommandStrings.PlaybackCategoryName)]
    [SupportedOSPlatform("windows")]
    internal class PlaybackCommands : BaseCommandModule
    {
        [Command("pause")]
        [Aliases("ps")]
        [Description("Pause playback")]
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
        }

        [Command("resume")]
        [Aliases("rs")]
        [Description("Resume playback")]
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
        }

        [Command("stop")]
        [Aliases("st")]
        [Description("Stop playback")]
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
        }

        [Command("skip")]
        [Aliases("s")]
        [Description("Skip current track")]
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
        }

        [Command("count")]
        [Aliases("cnt", "cn")]
        [Description("Get the number of tracks in the queue")]
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
        }

        [Command("clear")]
        [Aliases("clr", "cl", "c")]
        [Description("Clear track queue")]
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
        }

        [Command("seek")]
        [Aliases("sk")]
        [Description("Seek audio stream")]
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
                "mm:ss",
                "HH:mm:ss"
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
        }

        [Command("return")]
        [Aliases("rt")]
        [Description("Return the track to the queue")]
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
        }

        [Command("currenttrack")]
        [Aliases("track", "tr")]
        [Description("Get information about the current track")]
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
        }

        [Command("nexttrack")]
        [Aliases("next", "ntr", "nex")]
        [Description("Get information about the next track")]
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
        }
    }
}
