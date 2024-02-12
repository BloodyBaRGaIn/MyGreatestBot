using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class StopException : CommandExecutionException
    {
        public override string Title { get; } = "Stop";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Black;

        public StopException() : base() { }
        public StopException(string message) : base(message) { }
        public StopException(string message, Exception innerException) : base(message, innerException) { }
    }
}
