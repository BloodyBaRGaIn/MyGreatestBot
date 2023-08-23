using DicordNET.ApiClasses;
using DicordNET.Bot;
using DicordNET.Sql;
using DicordNET.Player;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

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
            if (ctx.Guild == null)
            {
                return;
            }
            await BotWrapper.Join(ctx);
        }

        [Command("leave")]
        [Aliases("l")]
        [Description("Leave voice channel")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task Leave(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }
            await BotWrapper.Leave(ctx);
        }

        [Command("logout")]
        [Aliases("bye")]
        [Description("Logout and exit")]
        [SuppressMessage("Performance", "CA1822")]
        
        public async Task LogoutCommand(CommandContext ctx)
        {
            if (ctx.Guild == null)
            {
                return;
            }

            _ = await ctx.Channel.SendMessageAsync(":wave:");
            await BotWrapper.Leave(ctx);

            DSharpPlus.DiscordClient? bot_client = BotWrapper.Client;

            if (bot_client != null)
            {
                await bot_client.UpdateStatusAsync(null, UserStatus.Offline);

                PlayerManager.Terminate();
                ApiConfig.DeinitApis();
                SqlServerWrapper.Close();

                await bot_client.DisconnectAsync();
                bot_client.Dispose();
            }

            Environment.Exit(0);
        }
    }
}
