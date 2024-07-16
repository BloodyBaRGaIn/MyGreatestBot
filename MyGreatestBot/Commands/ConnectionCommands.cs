using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System;
using System.Collections.Generic;
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

        [Command("apistatus")]
        [Aliases("status")]
        [Description("Get APIs status")]
        [SuppressMessage("Performance", "CA1822")]
        [RequiresDynamicCode("Calls System.Enum.GetValues(Type)")]
        public async Task StatusCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            string result = ApiManager.GetRegisteredApiStatus();

            handler.Message.Send(string.IsNullOrEmpty(result)
                ? new StatusException("No APIs initialized")
                : new StatusException(result).WithSuccess());

            await Task.Delay(1);
        }

        [Command("apireload")]
        [Aliases("reload")]
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

            if (!ApiManager.IsAnyApiFailed)
            {
                throw new ReloadException("No failed APIs to reload");
            }

            ApiManager.ReloadFailedApis();

            if (!ApiManager.IsAnyApiFailed)
            {
                handler.Message.Send(new ReloadException("Reload success").WithSuccess());
            }
            else
            {
                throw new ReloadException("Reload failed");
            }

            await Task.Delay(1);
        }

        [Command("playerstatus")]
        [Aliases("plst")]
        [Description("Get player status")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task PlayerStatusCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            await Task.Run(() => handler.PlayerInstance.GetStatus(CommandActionSource.Command));
        }

        private static async Task LogoutGeneric(CommandContext ctx, CommandActionSource source)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            if (DiscordWrapper.Client != null)
            {
                IEnumerable<DiscordUser>? owners = DiscordWrapper.Client.CurrentApplication.Owners;
                if (owners != null && !owners.Select(x => x.Id).Contains(ctx.User.Id))
                {
                    throw new InvalidOperationException("You are not allowed to execute this command");
                }
            }

            await ConnectionHandler.Logout(source);
        }

        [Command("logout")]
        [Aliases("exit", "quit", "bye", "bb")]
        [Description("Logout and exit")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task LogoutCommand(CommandContext ctx)
        {
            await LogoutGeneric(ctx, CommandActionSource.LogoutBye);
        }

        [Command("shutdown")]
        [Description("Force shutdown")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task ShutdownCommand(CommandContext ctx)
        {
            await LogoutGeneric(ctx, CommandActionSource.LogoutShut);
        }
    }
}
