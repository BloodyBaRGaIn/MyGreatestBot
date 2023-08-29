using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Bot;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
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
        public async Task JoinCommand(CommandContext ctx)
        {
            await ConnectionHandler.Join(ctx);
        }

        [Command("leave")]
        [Aliases("l")]
        [Description("Leave voice channel")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task LeaveCommand(CommandContext ctx)
        {
            await ConnectionHandler.Leave(ctx);
        }

        [Command("reload")]
        [Description("Reload failed APIs")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task ReloadCommand(CommandContext ctx)
        {
            if (ApiManager.FailedIntents == ApiIntents.None)
            {
                _ = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Reload",
                    Description = "No failed APIs to reload"
                });
                return;
            }

            ApiManager.ReloadFailedApis();

            if (ApiManager.FailedIntents == ApiIntents.None)
            {
                _ = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Blue,
                    Title = "Reload",
                    Description = "Reload success"
                });
            }
            else
            {
                _ = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Reload",
                    Description = "Reload failed"
                });
            }

            await Task.Delay(1);
        }

        [Command("logout")]
        [Aliases("bye", "bb", "b")]
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
                ApiManager.DeinitApis();

                await bot_client.DisconnectAsync();
                bot_client.Dispose();
            }

            Environment.Exit(0);
        }
    }
}
