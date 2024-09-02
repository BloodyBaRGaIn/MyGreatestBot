using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class SkipCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Skip";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blue;
        public SkipCommandException(string message) : base(message) { }
        public SkipCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
