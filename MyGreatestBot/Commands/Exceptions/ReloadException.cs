using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ReloadException : CommandExecutionException
    {
        public override string Title { get; } = "Reload";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blue;

        public ReloadException() : base() { }
        public ReloadException(string message) : base(message) { }
        public ReloadException(string message, Exception innerException) : base(message, innerException) { }
    }
}
