using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Services.Discord;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
{
    /// <summary>
    /// Connection commands
    /// </summary>
    [Category(CommandStrings.ConnectionCategoryName)]
    internal class ConnectionCommands : BaseCommandModule
    {
        [Command("join")]
        [Aliases("j")]
        [Description("Join voice channel")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task JoinCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler != null)
            {
                await handler.Join(ctx);
            }
        }

        [Command("leave")]
        [Aliases("l")]
        [Description("Leave voice channel")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task LeaveCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler != null)
            {
                await handler.Leave(ctx);
            }
        }

        [Command("reload")]
        [Description("Reload failed APIs")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task ReloadCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            if (ApiManager.FailedIntents == ApiIntents.None)
            {
                throw new ReloadException("No failed APIs to reload");
            }

            ApiManager.ReloadFailedApis();

            if (ApiManager.FailedIntents == ApiIntents.None)
            {
                handler.Message.Send(new ReloadException("Reload success"), is_success: true);
            }
            else
            {
                throw new ReloadException("Reload failed");
            }

            await Task.Delay(1);
        }

        [Command("logout")]
        [Aliases("exit", "quit", "bye", "bb", "b")]
        [Description("Logout and exit")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task LogoutCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            if (DiscordWrapper.Client != null
                && !DiscordWrapper.Client.CurrentApplication.Owners
                    .Select(x => x.Id).Contains(ctx.User.Id))
            {
                throw new CommandExecutionException("You are not allowed to execute this command");
            }

            handler.TextChannel = ctx.Channel;

            await ConnectionHandler.Logout();
        }
    }
}
