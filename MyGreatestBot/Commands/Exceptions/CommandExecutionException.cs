using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public abstract class CommandExecutionException : Exception
    {
        private bool IsSuccess { get; set; }

        protected static DiscordColor GenericColor { get; } = new(92, 45, 145);

        public new string Message => base.Message;
        public virtual string Title { get; } = "Command error";
        protected virtual DiscordColor ErroredColor { get; } = DiscordColor.Red;
        protected virtual DiscordColor ExecutedColor { get; } = DiscordColor.White;

        public DiscordColor Color => IsSuccess ? ExecutedColor : ErroredColor;

        protected CommandExecutionException() : base("Exception was thrown") { }
        protected CommandExecutionException(string message) : base(message) { }
        protected CommandExecutionException(string message, Exception exception) : base(message, exception) { }

        public CommandExecutionException WithSuccess()
        {
            IsSuccess = true;
            return this;
        }
    }
}
