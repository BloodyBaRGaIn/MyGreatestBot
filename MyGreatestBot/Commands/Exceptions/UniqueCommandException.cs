using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class UniqueCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Unique";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Orange;
        public UniqueCommandException(string message) : base(message) { }
        public UniqueCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
