using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace MyGreatestBot.Commands.Utils
{
    /// <summary>
    /// Help formatter
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        protected string _content;

        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
            _content = string.Empty;
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

        public static IEnumerable<CustomHelpFormatter> WithAllCommands(CommandContext ctx)
        {
            foreach (string item in MarkdownWriter.GetFullCommandsString())
            {
                yield return new CustomHelpFormatter(ctx) { _content = item };
            }
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(content: _content);
        }

        private void AddField(Command command)
        {
            _content += MarkdownWriter.GetCommandString(command);
        }
    }
}
