using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class DbSaveCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Save";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blurple;
        public DbSaveCommandException(string message) : base(message) { }
        public DbSaveCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
