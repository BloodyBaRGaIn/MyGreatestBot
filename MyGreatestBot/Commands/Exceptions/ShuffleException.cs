using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public class ShuffleException : CommandExecutionException
    {
        public override string Title => "Shuffle";
        public override DiscordColor ExecutedColor => DiscordColor.Orange;

        public ShuffleException() : base() { }
        public ShuffleException(string message) : base(message) { }
        public ShuffleException(string message, Exception innerException) : base(message, innerException) { }
    }
}
