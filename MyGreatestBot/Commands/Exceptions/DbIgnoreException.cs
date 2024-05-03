using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class DbIgnoreException : CommandExecutionException
    {
        public override string Title { get; } = "Ignore";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Yellow;

        public DbIgnoreException() : base() { }
        public DbIgnoreException(string message) : base(message) { }
        public DbIgnoreException(string message, Exception innerException) : base(message, innerException) { }
    }
}
