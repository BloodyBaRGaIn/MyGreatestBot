using DicordNET.Bot;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System.Reflection;

namespace DicordNET.Commands
{
    /// <summary>
    /// Help formatter
    /// </summary>
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
            foreach (var cmd in cmds)
            {
                AddField(cmd);
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
            if (_embed.Fields.Count == 0)
            {
                if (BotWrapper.Commands != null)
                {
                    foreach (var cmd in BotWrapper.Commands.RegisteredCommands.Values.DistinctBy(c => c.Name))
                    {
                        AddField(cmd);
                    }
                }
            }
            return new CommandHelpMessage(embed: _embed);
        }

        private void AddField(Command cmd)
        {
            string title = cmd.Name;
            List<string> arguments = new();

            if (cmd.Module != null)
            {
                var moduleType = cmd.Module.GetInstance(BotWrapper.BotInstance.ServiceProvider).GetType();
                var commandMethods = moduleType.GetMethods().Where(m =>
                    m.CustomAttributes.Any(a => a.AttributeType == typeof(CommandAttribute))
                    && m.CustomAttributes.Any(a => a.ConstructorArguments.Any(c =>
                        c.Value?.ToString() == cmd.Name)));

                var parameters = commandMethods.FirstOrDefault()?.GetParameters();

                var contextParameterCount = parameters?.Count(p => p.ParameterType == typeof(CommandContext)) ?? 0;
                if (contextParameterCount == 1)
                {
                    var argument_parameters = parameters?
                        .Where(p => p != null && p.ParameterType != typeof(CommandContext) && !string.IsNullOrWhiteSpace(p.Name));

                    foreach (ParameterInfo parameter in argument_parameters ?? Enumerable.Empty<ParameterInfo>())
                    {
                        var descriptionAttribute = parameter.CustomAttributes
                            .FirstOrDefault(a => a.AttributeType == typeof(DescriptionAttribute) && a.ConstructorArguments.Any());

                        string description = descriptionAttribute?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? string.Empty;
                        string fullParameter = $"{parameter.Name} ({parameter.ParameterType.Name})";

                        if (!string.IsNullOrWhiteSpace(description))
                        {
                            fullParameter += $" - {description}";
                        }

                        arguments.Add(fullParameter);
                    }
                }
            }

            if (cmd.Aliases != null && cmd.Aliases.Any())
            {
                title += $" ({string.Join(", ", cmd.Aliases)})";
            }

            string content = cmd.Description ?? string.Empty;

            if (arguments.Any())
            {
                content += string.IsNullOrWhiteSpace(content) ? "Arguments" : "\nArguments";
                content += string.Join("\n", arguments);
            }

            _embed.AddField(title, content);
        }
    }
}
