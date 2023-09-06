using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MyGreatestBot.Bot.Handlers;
using MyGreatestBot.Commands.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
{
    [Category(CommandStrings.DatabaseCategoryName)]
    [SupportedOSPlatform("windows")]
    internal class DatabaseCommands : BaseCommandModule
    {
        [Command("ignoretrack")]
        [Aliases("it")]
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
        }


        [Command("save")]
        [Description("Save tracks")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task SaveCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Save(CommandActionSource.Command));
        }

        [Command("restore")]
        [Description("Restore saved tracks")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task RestoreCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.Restore(CommandActionSource.Command));
        }
    }
}
