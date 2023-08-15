using DicordNET.Bot;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System.Reflection;

namespace DicordNET.Commands
{
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
            List<string> argumets = new();

            if (cmd.Module != null)
            {
                IEnumerable<ParameterInfo>? parameters = cmd.Module.GetInstance(BotWrapper.BotInstance.ServiceProvider)
                    .GetType()
                    .GetMethods()
                    .Where(m =>
                        m.CustomAttributes.Any(a => a.AttributeType == typeof(CommandAttribute))
                        && m.CustomAttributes.Select(a => a.ConstructorArguments)
                            .Any(c => c != null
                                && c.Any()
                                && c.Select(i => i.Value?.ToString() ?? string.Empty)
                                    .Contains(cmd.Name))).FirstOrDefault()?.GetParameters();

                if (parameters != null && parameters.Any() && parameters.Where(p => p.ParameterType == typeof(CommandContext)).Count() == 1)
                {
                    foreach (ParameterInfo? item in parameters.Where(p => p.ParameterType != typeof(CommandContext) && !string.IsNullOrWhiteSpace(p.Name)))
                    {
                        string full_item = $"{item.Name} ({item.ParameterType.Name})";
                        CustomAttributeData? descr_attr = item.CustomAttributes
                            .FirstOrDefault(a => a.AttributeType == typeof(DescriptionAttribute) && a.ConstructorArguments != null && a.ConstructorArguments.Any());
                        if (descr_attr != null)
                        {
                            string descr = descr_attr.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(descr))
                            {
                                full_item += $" - {descr}";
                            }
                        }
                        argumets.Add(full_item);
                    }
                }
            }

            if (cmd.Aliases != null && cmd.Aliases.Any())
            {
                title += $" ({string.Join(", ", cmd.Aliases)})";
            }
            string content = cmd.Description ?? string.Empty;

            if (argumets.Any())
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    content += "\n";
                }
                content += "Arguments";
                foreach (string arg in argumets)
                {
                    content += $"\n{arg}";
                }
            }

            _embed.AddField(title, content);
        }
    }
}
