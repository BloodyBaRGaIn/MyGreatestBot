using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class PauseException : CommandExecutionException
    {
        public override string Title { get; } = "Pause";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.LightGray;
        public PauseException(string message) : base(message) { }
        public PauseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
