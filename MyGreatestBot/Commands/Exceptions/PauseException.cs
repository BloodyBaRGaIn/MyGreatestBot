using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public class PauseException : CommandExecutionException
    {
        public override string Title => "Pause";
        public override DiscordColor ExecutedColor => DiscordColor.LightGray;

        public PauseException() : base() { }
        public PauseException(string message) : base(message) { }
        public PauseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
