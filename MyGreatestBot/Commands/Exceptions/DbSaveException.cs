using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class DbSaveException : CommandExecutionException
    {
        public override string Title { get; } = "Save";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blurple;

        public DbSaveException() : base() { }
        public DbSaveException(string message) : base(message) { }
        public DbSaveException(string message, Exception innerException) : base(message, innerException) { }
    }
}
