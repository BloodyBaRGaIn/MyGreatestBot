using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class SeekException : CommandExecutionException
    {
        public override string Title { get; } = "Seek";
        protected override DiscordColor ExecutedColor { get; } = GenericColor;

        public SeekException() : base() { }
        public SeekException(string message) : base(message) { }
        public SeekException(string message, Exception innerException) : base(message, innerException) { }
    }
}
