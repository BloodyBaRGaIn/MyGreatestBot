using DicordNET.Bot;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DicordNET.Commands
{
    [Category(CommandStrings.DebugCategoryName)]
    internal class DebugCommands : BaseCommandModule
    {
        [Command("test")]
        [Description("Get test message")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822")]
        public async Task TestCommand(CommandContext ctx)
        {
            _ = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder()
            {
                Color = DiscordColor.White,
                Title = "Test",
                Description = "Hello World from .NET"
            });
        }

        [Command("name")]
        [Description("Get origin bot name")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822")]
        public async Task NameCommand(CommandContext ctx)
        {
            var bot_client = BotWrapper.Client;
            if (bot_client == null)
            {
                _ = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Name",
                    Description = "Cannot get my username"
                });
            }
            else
            {
                _ = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.White,
                    Title = "Name",
                    Description = $"My name is {bot_client.CurrentUser.Username}"
                });
            }
        }
    }
}
