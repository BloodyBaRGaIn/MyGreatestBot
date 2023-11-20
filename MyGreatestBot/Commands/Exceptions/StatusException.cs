using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class StatusException : CommandExecutionException
    {
        public override string Title => "Status";
        public override DiscordColor ExecutedColor => DiscordColor.Blue;

        public StatusException() : base() { }
        public StatusException(string message) : base(message) { }
        public StatusException(string message, Exception innerException) : base(message, innerException) { }
    }
}
