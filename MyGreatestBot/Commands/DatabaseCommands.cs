using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MyGreatestBot.Commands.Utils;
using System.Threading.Tasks;

using AllowNullAttribute = System.Diagnostics.CodeAnalysis.AllowNullAttribute;
using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace MyGreatestBot.Commands
{
    [Category(CommandStrings.DatabaseCategoryName)]
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

            await Task.Run(() => handler.PlayerInstance.DbIgnoreTrack(CommandActionSource.Command));
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

            await Task.Run(() => handler.PlayerInstance.DbIgnoreArtist(artist_index, CommandActionSource.Command));
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

            await Task.Run(() => handler.PlayerInstance.DbSave(CommandActionSource.Command));
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

            await Task.Run(() => handler.PlayerInstance.DbRestore(CommandActionSource.Command));
        }
    }
}
