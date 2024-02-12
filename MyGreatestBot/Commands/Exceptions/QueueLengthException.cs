using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class QueueLengthException : CommandExecutionException
    {
        public override string Title { get; } = "Count";
        protected override DiscordColor ExecutedColor { get; } = GenericColor;

        public QueueLengthException() : base() { }
        public QueueLengthException(string message) : base(message) { }
        public QueueLengthException(string message, Exception innerException) : base(message, innerException) { }
    }
}
