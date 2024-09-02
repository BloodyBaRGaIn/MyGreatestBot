using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class PlayerStatusCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Player";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blue;
        public PlayerStatusCommandException(string message) : base(message) { }
        public PlayerStatusCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
