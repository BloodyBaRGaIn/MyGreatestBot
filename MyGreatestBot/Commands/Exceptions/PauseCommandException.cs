using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class PauseCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Pause";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.LightGray;
        public PauseCommandException(string message) : base(message) { }
        public PauseCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
