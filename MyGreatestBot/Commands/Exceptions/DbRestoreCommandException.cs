using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class DbRestoreCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Restore";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blurple;
        public DbRestoreCommandException(string message) : base(message) { }
        public DbRestoreCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
