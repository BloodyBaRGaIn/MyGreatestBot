using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;

namespace MyGreatestBot.Commands.Utils
{
    public abstract class PlainHelpFormatter : BaseHelpFormatter
    {
        /// <inheritdoc cref="BaseHelpFormatter.Context"/>
        protected new CommandContext? Context { get; }

        /// <inheritdoc cref="BaseHelpFormatter.CommandsNext"/>
        protected new CommandsNextExtension? CommandsNext => Context?.CommandsNext;

        /// <inheritdoc cref="BaseHelpFormatter(CommandContext)"/>
#pragma warning disable CS8604
        public PlainHelpFormatter(CommandContext? ctx = null) : base(ctx)
#pragma warning restore CS8604
        {

        }
    }
}
