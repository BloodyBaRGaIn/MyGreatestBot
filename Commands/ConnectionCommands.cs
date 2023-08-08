using DicordNET.ApiClasses;
using DicordNET.Bot;
using DicordNET.Player;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DicordNET.Commands
{
    [Category(CommandStrings.ConnectionCategoryName)]
    internal class ConnectionCommands : BaseCommandModule
    {
        [Command("join")]
        [Aliases("j")]
        [Description("Join voice channel")]
        public async Task Join(CommandContext ctx)
        {
            await BotWrapper.Join(ctx);
        }

        [Command("leave")]
        [Aliases("l")]
        [Description("Leave voice channel")]
        public async Task Leave(CommandContext ctx)
        {
            await BotWrapper.Leave(ctx);
        }

        [Command("logout")]
        [Aliases("bye")]
        [Description("Logout and exit")]
        public async Task LogoutCommand(CommandContext ctx)
        {
            _ = await ctx.Channel.SendMessageAsync(":wave:");
            var bot_client = BotWrapper.Client;

            if (bot_client != null)
            {
                await bot_client.UpdateStatusAsync(null, UserStatus.Offline);

                PlayerManager.Terminate();
                ApiConfig.DeinitApis();

                await bot_client.DisconnectAsync();
                bot_client.Dispose();
            }

            Environment.Exit(0);
        }
    }
}
