using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.Commands.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
{
    [Category(CommandStrings.QueuingCategoryName)]
    internal class QueuingCommands : BaseCommandModule
    {
        private static async Task PlayCommandGeneric(CommandContext ctx, string query, CommandActionSource source)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            Stopwatch command_stopwatch = new();

            command_stopwatch.Start();
            handler.TextChannel = ctx.Channel;
            handler.Voice.UpdateVoiceConnection();

            if (handler.VoiceConnection == null)
            {
                await handler.Join(ctx);
                await handler.Voice.WaitForConnectionAsync();
                handler.Update(ctx.Guild);
            }
            command_stopwatch.Stop();

            handler.Log.Send($"Preparation takes {command_stopwatch.ElapsedMilliseconds} ms.", LogLevel.Debug);

            command_stopwatch.Restart();
            IEnumerable<BaseTrackInfo> tracks = ApiManager.GetAll(query);
            command_stopwatch.Stop();

            handler.Log.Send($"GetTracks takes {command_stopwatch.ElapsedMilliseconds} ms.", LogLevel.Debug);

            command_stopwatch.Restart();
            await Task.Run(() => handler.PlayerInstance.Enqueue(ref tracks, source));
            command_stopwatch.Stop();

            handler.Log.Send($"Enqueue takes {command_stopwatch.ElapsedMilliseconds} ms.", LogLevel.Debug);
        }

        [Command("play")]
        [Aliases("p")]
        [Description("Add tracks to the queue")]
        [Example($"{DiscordWrapper.DefaultPrefix}play URL")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task PlayCommand(
            CommandContext ctx,
            [Description("URL")] string query,
            [AllowNull,
            Description(
            "Additional queuing paramtetrs (optional)\r\n" +
            "\t\t\\SH - shuffle\r\n" +
            "\t\t\\FF - enqueue to the head\r\n" +
            "\t\t\\T - play immediatly\r\n" +
            "\t\t\\R - play in radio mode\r\n" +
            "\t\t\\B - bypass SQL check")] params string[] args)
        {
            CommandActionSource source = CommandActionSource.Command;
            if (args != null)
            {
                bool start_args = false;
                for (int i = 0; i < args.Length;)
                {
                    string arg = args[i];
                    if (string.IsNullOrWhiteSpace(arg))
                    {
                        continue;
                    }

                    if (start_args)
                    {
                        string u_arg = arg.ToUpperInvariant();
                        switch (u_arg)
                        {
                            case "\\SH":
                            case "\\SHUFFLE":
                                source |= CommandActionSource.PlayerShuffle;
                                break;
                            case "\\FF":
                            case "\\HEAD":
                                source |= CommandActionSource.PlayerToHead;
                                break;
                            case "\\T":
                                source |= CommandActionSource.PlayerToHead;
                                source |= CommandActionSource.PlayerSkipCurrent;
                                break;
                            case "\\R":
                            case "\\RADIO":
                                source |= CommandActionSource.PlayerRadio;
                                break;
                            case "\\B":
                            case "\\BYPASS":
                                source |= CommandActionSource.PlayerNoBlacklist;
                                break;

                            default:
                                // skip unknown arguments
                                break;
                        }
                    }
                    else
                    {
                        if (arg.StartsWith('\\'))
                        {
                            start_args = true;
                            continue;
                        }

                        query = string.Join(' ', query, arg);
                    }

                    i++;
                }
            }

            await PlayCommandGeneric(ctx, query, source);
        }

        [Command("playshuffled")]
        [Aliases("psh")]
        [Description("Add shuffled tracks to the queue")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task PlayShuffledCommand(
            CommandContext ctx,
            [RemainingText, Description("URL")] string query)
        {
            await PlayCommandGeneric(ctx, query,
                CommandActionSource.Command | CommandActionSource.PlayerShuffle);
        }

        [Command("playhead")]
        [Aliases("pf", "ff", "f")]
        [Description("Add tracks to the head")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task HeadCommand(
            CommandContext ctx,
            [RemainingText, Description("URL")] string query)
        {
            await PlayCommandGeneric(ctx, query,
                CommandActionSource.Command | CommandActionSource.PlayerToHead);
        }

        [Command("playimmediatly")]
        [Aliases("pi", "t", "r")]
        [Description("Play the track immediatly")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task TmsCommand(
            CommandContext ctx,
            [RemainingText, Description("URL")] string query)
        {
            await PlayCommandGeneric(ctx, query,
                CommandActionSource.Command | CommandActionSource.PlayerToHead | CommandActionSource.PlayerSkipCurrent);
        }

        [Command("playradio")]
        [Aliases("radio", "pr")]
        [Description("Play in radio mode")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task PlayRadioCommand(
            CommandContext ctx,
            [RemainingText, Description("URL")] string query)
        {
            await PlayCommandGeneric(ctx, query,
                CommandActionSource.Command | CommandActionSource.PlayerRadio);
        }

        [Command("playbypass")]
        [Aliases("pb", "b")]
        [Description("Play the track without check")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task PlayBypassCommand(
            CommandContext ctx,
            [RemainingText, Description("URL")] string query)
        {
            await PlayCommandGeneric(ctx, query,
                CommandActionSource.Command | CommandActionSource.PlayerNoBlacklist);
        }
    }
}
