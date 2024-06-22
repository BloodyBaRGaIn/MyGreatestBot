using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ReturnException : CommandExecutionException
    {
        public override string Title { get; } = "Return";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Yellow;
        public ReturnException(string message) : base(message) { }
        public ReturnException(string message, Exception innerException) : base(message, innerException) { }
    }
}
