using DicordNET.ApiClasses;
using DicordNET.Bot;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace DicordNET.Commands
{
    /// <summary>
    /// Connection commands
    /// </summary>
    [Category(CommandStrings.ConnectionCategoryName)]
    [SupportedOSPlatform("windows")]
    internal class ConnectionCommands : BaseCommandModule
    {
        [Command("join")]
        [Aliases("j")]
        [Description("Join voice channel")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Join(CommandContext ctx)
        {
            await ConnectionHandler.Join(ctx);
        }

        [Command("leave")]
        [Aliases("l")]
        [Description("Leave voice channel")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Leave(CommandContext ctx)
        {
            await ConnectionHandler.Leave(ctx);
        }

        [Command("logout")]
        [Aliases("bye")]
        [Description("Logout and exit")]
        [SuppressMessage("Performance", "CA1822")]

        public async Task LogoutCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            _ = await ctx.Channel.SendMessageAsync(":wave:");
            await ConnectionHandler.Leave(ctx);

            DSharpPlus.DiscordClient? bot_client = BotWrapper.Client;

            if (bot_client != null)
            {
                await bot_client.UpdateStatusAsync(null, UserStatus.Offline);

                handler.PlayerInstance.Terminate();
                ApiConfig.DeinitApis();

                await bot_client.DisconnectAsync();
                bot_client.Dispose();
            }

            Environment.Exit(0);
        }
    }
}
