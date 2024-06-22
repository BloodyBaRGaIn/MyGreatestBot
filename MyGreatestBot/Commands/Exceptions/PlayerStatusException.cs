using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class PlayerStatusException : CommandExecutionException
    {
        public override string Title { get; } = "Player";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blue;
        public PlayerStatusException(string message) : base(message) { }
        public PlayerStatusException(string message, Exception innerException) : base(message, innerException) { }
    }
}
