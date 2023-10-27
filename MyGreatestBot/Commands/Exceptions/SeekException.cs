using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class SeekException : CommandExecutionException
    {
        public override string Title => "Seek";
        public override DiscordColor ExecutedColor => GenericColor;

        public SeekException() : base() { }
        public SeekException(string message) : base(message) { }
        public SeekException(string message, Exception innerException) : base(message, innerException) { }
    }
}
