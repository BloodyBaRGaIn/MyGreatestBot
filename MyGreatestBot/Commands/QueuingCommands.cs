using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

            handler.TextChannel = ctx.Channel;
            handler.Voice.UpdateVoiceConnection();

            if (handler.VoiceConnection == null)
            {
                await handler.Join(ctx);
                await handler.Voice.WaitForConnectionAsync();
                handler.Update(ctx.Guild);
            }

            IEnumerable<ITrackInfo> tracks = ApiManager.GetAll(query);

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks, source));
        }

        [Command("play")]
        [Aliases("p")]
        [Description("Add tracks to the queue")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task PlayCommand(
            CommandContext ctx,
            [Description("URL")] string query,
            [AllowNull,
            Description(
            "Additional queuing paramtetrs (optional)```\r\n" +
            "\t\t- ```\\SH - shuffle```\r\n" +
            "\t\t- ```\\FF - enqueue to the head```\r\n" +
            "\t\t- ```\\T - play immediatly```\r\n" +
            "\t\t- ```\\B - bypass SQL check")] params string[] args)
        {
            CommandActionSource source = CommandActionSource.Command;
            if (args != null)
            {
                bool start_args = false;
                foreach (string arg in args)
                {
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
                                source |= CommandActionSource.PlayerToHead | CommandActionSource.PlayerSkipCurrent;
                                break;

                            case "\\B":
                                source |= CommandActionSource.PlayerNoBlacklist;
                                break;

                            default:
                                // skip unknown arguments
                                break;
                        }
                    }
                    else
                    {
                        if (arg.StartsWith("\\"))
                        {
                            start_args = true;
                            continue;
                        }

                        query = string.Join(' ', query, arg);
                    }
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

        [Command("head")]
        [Aliases("ff")]
        [Description("Add tracks to the head")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task HeadCommand(
            CommandContext ctx,
            [RemainingText, Description("URL")] string query)
        {
            await PlayCommandGeneric(ctx, query,
                CommandActionSource.Command | CommandActionSource.PlayerToHead);
        }

        [Command("tms")]
        [Aliases("t")]
        [Description("Play the track immediatly")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task TmsCommand(
            CommandContext ctx,
            [RemainingText, Description("URL")] string query)
        {
            await PlayCommandGeneric(ctx, query,
                CommandActionSource.Command | CommandActionSource.PlayerToHead | CommandActionSource.PlayerSkipCurrent);
        }

        [Command("playbypass")]
        [Aliases("pb")]
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
