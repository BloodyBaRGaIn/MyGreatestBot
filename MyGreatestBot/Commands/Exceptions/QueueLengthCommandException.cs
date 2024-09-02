using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class QueueLengthCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Count";
        protected override DiscordColor ExecutedColor { get; } = GenericColor;
        public QueueLengthCommandException(string message) : base(message) { }
        public QueueLengthCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
