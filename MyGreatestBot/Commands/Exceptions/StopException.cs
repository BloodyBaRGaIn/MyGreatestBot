using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public class StopException : CommandExecutionException
    {
        public override string Title => "Stop";
        public override DiscordColor ExecutedColor => DiscordColor.Black;

        public StopException() : base() { }
        public StopException(string message) : base(message) { }
        public StopException(string message, Exception innerException) : base(message, innerException) { }
    }
}
