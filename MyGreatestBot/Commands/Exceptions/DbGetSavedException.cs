using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class DbGetSavedException : CommandExecutionException
    {
        public override string Title { get; } = "Saved count";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blurple;
        public DbGetSavedException(string message) : base(message) { }
        public DbGetSavedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
