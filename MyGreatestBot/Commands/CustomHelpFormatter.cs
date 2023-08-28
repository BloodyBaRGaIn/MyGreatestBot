using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using MyGreatestBot.Bot;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MyGreatestBot.Commands
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

        public override CommandHelpMessage Build()
        {
            _ = _embed.ClearFields();
            if (BotWrapper.Commands != null)
            {
                foreach (Command? cmd in BotWrapper.Commands.RegisteredCommands.Values.DistinctBy(c => c.Name))
                {
                    AddField(cmd);
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
                IEnumerable<ParameterInfo> parameters = cmd.Module.GetInstance(BotWrapper.ServiceProvider)
                    .GetType().GetMethods().FirstOrDefault(m =>
                        m.CustomAttributes.FirstOrDefault(a =>
                            a.AttributeType == typeof(CommandAttribute))?.ConstructorArguments?.Any(c =>
                                c.Value?.ToString() == cmd.Name) ?? false)?.GetParameters()
                    ?? Enumerable.Empty<ParameterInfo>();

                int contextParameterCount = parameters.Count(p => p.ParameterType == typeof(CommandContext));
                if (contextParameterCount == 1 && parameters.Count() - contextParameterCount > 0)
                {
                    IEnumerable<ParameterInfo> argument_parameters = parameters
                        .Where(p =>
                            p != null
                            && p.ParameterType != typeof(CommandContext)
                            && !string.IsNullOrWhiteSpace(p.Name));

                    foreach (ParameterInfo parameter in argument_parameters)
                    {
                        CustomAttributeData? descriptionAttribute = parameter.CustomAttributes
                            .FirstOrDefault(a => a.AttributeType == typeof(DescriptionAttribute) && a.ConstructorArguments.Any());

                        string? description = descriptionAttribute?.ConstructorArguments.FirstOrDefault().Value?.ToString();
                        string fullParameter = $"{parameter.Name} ({parameter.ParameterType.Name})";

                        if (!string.IsNullOrWhiteSpace(description))
                        {
                            fullParameter += $" - {description}";
                        }

                        if (parameter.CustomAttributes.Any(a => a.AttributeType == typeof(OptionalAttribute)
                            || a.AttributeType == typeof(AllowNullAttribute)))
                        {
                            fullParameter += " (*optional*)";
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
                content += string.IsNullOrWhiteSpace(content) ? "**Arguments" : "\n**Arguments";
                content += ":**\n";
                content += string.Join("\n", arguments);
            }

            _ = _embed.AddField(title, content);
        }
    }
}
