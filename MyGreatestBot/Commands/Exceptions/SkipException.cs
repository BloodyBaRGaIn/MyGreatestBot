using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class SkipException : CommandExecutionException
    {
        public override string Title { get; } = "Skip";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blue;
        public SkipException(string message) : base(message) { }
        public SkipException(string message, Exception innerException) : base(message, innerException) { }
    }
}
