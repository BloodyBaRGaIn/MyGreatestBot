using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ReloadCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Reload";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blue;
        public ReloadCommandException(string message) : base(message) { }
        public ReloadCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
