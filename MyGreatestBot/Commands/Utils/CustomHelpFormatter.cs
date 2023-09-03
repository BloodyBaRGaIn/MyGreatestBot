using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using MyGreatestBot.Bot;
using MyGreatestBot.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace MyGreatestBot.Commands.Utils
{
    /// <summary>
    /// Help formatter
    /// </summary>

    [SupportedOSPlatform("windows")]
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        protected DiscordEmbedBuilder _embed;

        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
            _embed = new DiscordEmbedBuilder().WithColor(DiscordColor.Cyan).WithTitle("Help");
        }

        public override CustomHelpFormatter WithCommand(Command command)
        {
            AddField(command);

            return this;
        }

        public override CustomHelpFormatter WithSubcommands(IEnumerable<Command> cmds)
        {
            foreach (Command cmd in cmds)
            {
                AddField(cmd);
            }

            return this;
        }

        private CustomHelpFormatter WithSubcommands(IEnumerable<Command> cmds, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                categoryName = "Unnamed";
            }
            _ = _embed.AddField(categoryName.FirstCharToUpper(), "Commands:");

            return WithSubcommands(cmds);
        }

        public static IEnumerable<CustomHelpFormatter> WithAllCommands(CommandContext ctx)
        {
            if (BotWrapper.Commands == null)
            {
                yield break;
            }

            foreach (IGrouping<string, Command> category in BotWrapper.Commands.RegisteredCommands.Values
                .GroupBy(c => c.Category ?? string.Empty))
            {
                yield return new CustomHelpFormatter(ctx).WithSubcommands(category.DistinctBy(c => c.Name.ToLowerInvariant()), category.Key);
            }
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: _embed);
        }

        private void AddField(Command command)
        {
            string title = command.Name;
            string content = string.Empty;

            if (command.Aliases.Any())
            {
                title += $" ({string.Join(", ", command.Aliases)})";
            }

            if (!string.IsNullOrWhiteSpace(command.Description))
            {
                content += $"{command.Description}";
            }

            CommandOverload overload = command.Overloads[0];
            if (overload.Arguments.Any())
            {
                content += "\r\n**Arguments:**";
            }

            foreach (var argument in overload.Arguments)
            {
                content += $"\r\n{argument.Name} ({argument.Type.Name})";
                if (!string.IsNullOrWhiteSpace(argument.Description))
                {
                    content += $" - {argument.Description}";
                }
                if (argument.IsOptional)
                {
                    content += " (*optional*)";
                }
            }

            _ = _embed.AddField(title, content);
        }
    }
}
