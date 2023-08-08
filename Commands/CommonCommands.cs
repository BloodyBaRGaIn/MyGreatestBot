using DicordNET.Bot;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DicordNET.Commands
{
    [Category(CommandStrings.CommonCategoryName)]
    internal class CommonCommands : BaseCommandModule
    {
        [Command(DiscordBot.HelpCommandName)]
        [Aliases("h")]
        [Description("Get help")]
        public async Task HelpCommand(CommandContext ctx, [RemainingText, Description("Command name or alias")] string? helpText)
        {
            var command_dict = BotWrapper.Commands?.RegisteredCommands.DistinctBy(c => c.Value);

            string result = string.Empty;

            foreach (string cat in CommandStrings.CategoriesOrder)
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
