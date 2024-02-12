using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class IgnoreException : CommandExecutionException
    {
        public override string Title { get; } = "Ignore";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Yellow;

        public IgnoreException() : base() { }
        public IgnoreException(string message) : base(message) { }
        public IgnoreException(string message, Exception innerException) : base(message, innerException) { }
    }
}
