using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public class SeekException : CommandExecutionException
    {
        public override string Title => "Seek";
        public override DiscordColor ExecutedColor => DiscordColor.Purple;

        public SeekException() : base() { }
        public SeekException(string message) : base(message) { }
        public SeekException(string message, Exception innerException) : base(message, innerException) { }
    }
}
