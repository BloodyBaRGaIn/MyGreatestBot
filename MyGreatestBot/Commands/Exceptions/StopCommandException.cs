using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class StopCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Stop";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Black;
        public StopCommandException(string message) : base(message) { }
        public StopCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
