global using DiscordColor = DSharpPlus.Entities.DiscordColor;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public abstract class CommandExecutionException : Exception
    {
        private bool IsSuccess { get; set; }

        protected static DiscordColor GenericColor { get; } = new(92, 45, 145);

        public abstract string Title { get; }
        protected virtual DiscordColor ErroredColor { get; } = DiscordColor.Red;
        protected virtual DiscordColor ExecutedColor { get; } = DiscordColor.White;

        public DiscordColor Color => IsSuccess ? ExecutedColor : ErroredColor;

        protected CommandExecutionException(string message) : base(message) { }
        protected CommandExecutionException(string message, Exception exception) : base(message, exception) { }

        public CommandExecutionException WithSuccess()
        {
            IsSuccess = true;
            return this;
        }
    }
}
