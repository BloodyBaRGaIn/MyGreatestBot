using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ShuffleException : CommandExecutionException
    {
        public override string Title { get; } = "Shuffle";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Orange;

        public ShuffleException() : base() { }
        public ShuffleException(string message) : base(message) { }
        public ShuffleException(string message, Exception innerException) : base(message, innerException) { }
    }
}
