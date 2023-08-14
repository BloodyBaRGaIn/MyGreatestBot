using DicordNET.Bot;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DicordNET.Commands
{
    [Category(CommandStrings.DebugCategoryName)]
    internal class DebugCommands : BaseCommandModule
    {
        [Command("help")]
        [Aliases("h")]
        [Description("Get help")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822")]
        public async Task HelpCommand(CommandContext ctx, [RemainingText] string? command_str)
        {
            CustomHelpFormatter? custom = null;
            if (!string.IsNullOrWhiteSpace(command_str) && BotWrapper.Commands != null)
            {
                DSharpPlus.CommandsNext.Command cmd = BotWrapper.Commands.RegisteredCommands.ContainsKey(command_str)
                    ? BotWrapper.Commands.RegisteredCommands[command_str]
                    : throw new ArgumentException("Invalid command");
                custom = new CustomHelpFormatter(ctx).WithCommand(cmd) as CustomHelpFormatter;
            }
            custom ??= new(ctx);
            _ = await ctx.Channel.SendMessageAsync(custom.Build().Embed);
        }

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
            DSharpPlus.DiscordClient? bot_client = BotWrapper.Client;
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
