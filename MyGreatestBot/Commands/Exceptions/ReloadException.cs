using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ReloadException : CommandExecutionException
    {
        public override string Title => "Reload";
        public override DiscordColor ExecutedColor => DiscordColor.Blue;

        public ReloadException() : base() { }
        public ReloadException(string message) : base(message) { }
        public ReloadException(string message, Exception innerException) : base(message, innerException) { }
    }
}
