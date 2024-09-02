using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ReturnCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Return";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Yellow;
        public ReturnCommandException(string message) : base(message) { }
        public ReturnCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
