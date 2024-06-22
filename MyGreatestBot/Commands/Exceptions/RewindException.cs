using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class RewindException : CommandExecutionException
    {
        public override string Title { get; } = "Seek";
        protected override DiscordColor ExecutedColor { get; } = GenericColor;
        public RewindException(string message) : base(message) { }
        public RewindException(string message, Exception innerException) : base(message, innerException) { }
    }
}
