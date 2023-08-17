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
        [Description("Get \"Hello World\" response message")]
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

        [Command("help")]
        [Aliases("h")]
        [Description("Get help")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822")]
        public async Task HelpCommand(CommandContext ctx, [RemainingText, Description("Command name")] string? command)
        {
            CustomHelpFormatter? custom = null;
            if (!string.IsNullOrWhiteSpace(command) && BotWrapper.Commands != null)
            {
                string command_key = command.ToLowerInvariant();
                Command cmd = BotWrapper.Commands.RegisteredCommands.ContainsKey(command_key)
                    ? BotWrapper.Commands.RegisteredCommands[command_key]
                    : throw new ArgumentException("Invalid command");
                custom = new CustomHelpFormatter(ctx).WithCommand(cmd);
            }
            custom ??= new(ctx);
            _ = await ctx.Channel.SendMessageAsync(custom.Build().Embed);
        }
    }
}
