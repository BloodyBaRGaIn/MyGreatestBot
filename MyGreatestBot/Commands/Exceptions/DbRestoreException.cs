using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class DbRestoreException : CommandExecutionException
    {
        public override string Title { get; } = "Restore";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blurple;
        public DbRestoreException(string message) : base(message) { }
        public DbRestoreException(string message, Exception innerException) : base(message, innerException) { }
    }
}
