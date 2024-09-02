using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ShuffleCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Shuffle";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Orange;
        public ShuffleCommandException(string message) : base(message) { }
        public ShuffleCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
