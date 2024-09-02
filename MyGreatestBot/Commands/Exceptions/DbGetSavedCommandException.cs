using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class DbGetSavedCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Saved count";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blurple;
        public DbGetSavedCommandException(string message) : base(message) { }
        public DbGetSavedCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
