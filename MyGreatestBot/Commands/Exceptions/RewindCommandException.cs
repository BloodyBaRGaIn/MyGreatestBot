using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class RewindCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Seek";
        protected override DiscordColor ExecutedColor { get; } = GenericColor;
        public RewindCommandException(string message) : base(message) { }
        public RewindCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
