using DicordNET.Bot;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

namespace DicordNET.Commands
{
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        protected DiscordEmbedBuilder _embed;

        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
            _embed = new DiscordEmbedBuilder();

            // Help formatters do support dependency injection.
            // Any required services can be specified by declaring constructor parameters. 

            // Other required initialization here ...
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            AddField(command);

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> cmds)
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

            var module = cmd.Module;
            if (module != null)
            {
                var methods = module.GetInstance(BotWrapper.BotInstance.ServiceProvider).GetType().GetMethods();
                var cmd_attr = methods.Where(m => (m.CustomAttributes.Select(a => a.AttributeType.FullName).Contains(typeof(CommandAttribute).FullName))
                && (m.CustomAttributes.Select(a => a.ConstructorArguments).Where(c => c != null && c.Count != 0 && c.Select(i => i.Value?.ToString() ?? string.Empty).Contains(cmd.Name))).Any()).FirstOrDefault();
                if (cmd_attr != null)
                {
                    var parameters = cmd_attr.GetParameters();
                    foreach (var item in parameters)
                    {
                        if (item.ParameterType.FullName != typeof(CommandContext).FullName)
                        {
                            
                            if (!string.IsNullOrWhiteSpace(item.Name))
                            {
                                string full_item = $"{item.Name} ({item.ParameterType.Name})";
                                var descr_attr = item.CustomAttributes.Where(a => a.AttributeType.FullName == typeof(DescriptionAttribute).FullName && a.ConstructorArguments != null && a.ConstructorArguments.Any()).FirstOrDefault();
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
                }
            }
            if (cmd.Aliases!= null && cmd.Aliases.Any())
            {
                title += $" ({cmd.Aliases[0]}";
                for (int i = 1; i < cmd.Aliases.Count; i++)
                {
                    title += cmd.Aliases[i] + ", ";
                }
                title += ")";
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
