using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ClearCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Clear";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Aquamarine;
        public ClearCommandException(string message) : base(message) { }
        public ClearCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
