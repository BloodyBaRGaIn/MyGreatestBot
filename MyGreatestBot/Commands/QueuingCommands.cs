using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Bot.Handlers;
using MyGreatestBot.Commands.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
{
    [Category(CommandStrings.QueuingCategoryName)]
    [SupportedOSPlatform("windows")]
    internal class QueuingCommands : BaseCommandModule
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
        [Description("Add tracks to the queue")]
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

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks,
                CommandActionSource.Command));
        }

        [Command("playshuffled")]
        [Aliases("psh")]
        [Description("Add shuffled tracks to the queue")]
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

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks,
                CommandActionSource.Command | CommandActionSource.PlayerShuffle));
        }

        [Command("tms")]
        [Aliases("t")]
        [Description("Play the track immediatly")]
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

            await Task.Run(() => handler.PlayerInstance.Enqueue(tracks,
                CommandActionSource.Command | CommandActionSource.PlayerToHead));
        }
    }
}
