using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
{
    /// <summary>
    /// Connection commands.
    /// </summary>
    [Category(CommandStrings.ConnectionCategoryName)]
    internal class ConnectionCommands : BaseCommandModule
    {
        [Command("join"), Aliases("j")]
        [Description("Join voice channel")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task JoinCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler != null)
            {
                await handler.Join(ctx);
            }
        }

        [Command("leave"), Aliases("l")]
        [Description("Leave voice channel")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task LeaveCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler != null)
            {
                await handler.Leave(ctx);
            }
        }

        [Command("apistatus"), Aliases("status")]
        [Description("Get APIs status")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
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
                ? new ApiStatusCommandException("No APIs initialized")
                : new ApiStatusCommandException(result).WithSuccess());

            await Task.Delay(1);
        }

        [Command("apiinit"), Aliases("init")]
        [Description("Force API initialization")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task InitCommand(CommandContext ctx,
            string api)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            EnsureUserIsOwner(ctx.User);

            api = api.ToLowerInvariant().FirstCharToUpper();

            if (!Enum.TryParse(api, out ApiIntents intents))
            {
                handler.Message.Send(new ApiStatusCommandException($"Cannot find API \"{api}\""));
                return;
            }

            if (intents == ApiIntents.None)
            {
                handler.Message.Send(new ApiStatusCommandException("No API provided"));
                return;
            }

            ApiManager.InitApis(intents);

            await Task.Delay(1);
        }

        [Command("apideinit"), Aliases("deinit")]
        [Description("Force API deinitialization")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task DeinitCommand(CommandContext ctx,
            string api)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            EnsureUserIsOwner(ctx.User);

            api = api.ToLowerInvariant().FirstCharToUpper();

            if (!Enum.TryParse(api, out ApiIntents intents))
            {
                handler.Message.Send(new ApiStatusCommandException($"Cannot find API \"{api}\""));
                return;
            }

            if (intents == ApiIntents.None)
            {
                handler.Message.Send(new ApiStatusCommandException("No API provided"));
                return;
            }

            ApiManager.DeinitApis(intents);

            await Task.Delay(1);
        }

        [Command("apireload"), Aliases("reload")]
        [Description("Reload failed APIs")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
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
                throw new ReloadCommandException("No failed APIs to reload");
            }

            ApiManager.ReloadFailedApis();

            if (!ApiManager.IsAnyApiFailed)
            {
                handler.Message.Send(new ReloadCommandException("Reload success").WithSuccess());
            }
            else
            {
                throw new ReloadCommandException("Reload failed");
            }

            await Task.Delay(1);
        }

        [Command("playerstatus"), Aliases("plst")]
        [Description("Get player status")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
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

        [Command("logout"), Aliases("exit", "quit", "bye", "bb")]
        [Description("Logout and exit")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task LogoutCommand(CommandContext ctx)
        {
            await LogoutGeneric(ctx, CommandActionSource.LogoutBye);
        }

        [Command("shutdown")]
        [Description("Force shutdown")]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task ShutdownCommand(CommandContext ctx)
        {
            await LogoutGeneric(ctx, CommandActionSource.LogoutShut);
        }

        private static async Task LogoutGeneric(CommandContext ctx, CommandActionSource source)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            EnsureUserIsOwner(ctx.User);

            await ConnectionHandler.Logout(source);
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> 
        /// if provided <paramref name="user"/> is not application owner.
        /// </summary>
        /// <param name="user">
        /// User to be cheched.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// An exception with "not allowed action" message.
        /// </exception>
        private static void EnsureUserIsOwner(DiscordUser user)
        {
            IEnumerable<DiscordUser>? owners = DiscordWrapper.Client?.CurrentApplication?.Owners;
            if (owners != null && !owners.Select(x => x.Id).Contains(user.Id))
            {
                throw new InvalidOperationException("You are not allowed to execute this command");
            }
        }
    }
}
