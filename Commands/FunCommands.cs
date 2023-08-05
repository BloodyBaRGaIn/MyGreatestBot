using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;

namespace DicordNET.Commands
{
    internal class FunCommands : BaseCommandModule
    {
        private static readonly string[] Categories = new[]
        {
            "common",
            "connection",
            "player",
            "debug"
        };

        [Command("test")]
        [Category("debug")]
        [Description("Get test message")]
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
        [Category("debug")]
        [Description("Get origin bot name")]
        public async Task NameCommand(CommandContext ctx)
        {
            var bot_client = StaticBotInstanceContainer.Client;
            if (bot_client == null)
            {
                _ = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.White,
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
        [Category("common")]
        [Description("Get help")]
        public async Task HelpCommand(CommandContext ctx, [RemainingText, Description("Command name or alias")] string? helpText)
        {
            var command_dict = StaticBotInstanceContainer.Commands?.RegisteredCommands.DistinctBy(c => c.Value);

            string result = string.Empty;

            foreach (string cat in Categories)
            {
                var group = command_dict?.Where(c => c.Value.Category == cat);
                if (group?.Any() ?? false)
                {
                    result += cat + "\n";
                    int count = group.Count();
                    for (int i = 0; i < count; i++)
                    {
                        var pair = group.ElementAt(i);
                        var command = pair.Value;
                        result += $"{command.Name}";
                        foreach (var alias in command.Aliases)
                        {
                            result += $" {alias}";
                        }

                        result += $" : {command.Description}\n";
                    }
                }
            }

            await ctx.Channel.SendMessageAsync(result);
        }
    }
}
