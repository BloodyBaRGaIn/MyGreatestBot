using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class DbIgnoreCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Ignore";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Yellow;
        public DbIgnoreCommandException(string message) : base(message) { }
        public DbIgnoreCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
