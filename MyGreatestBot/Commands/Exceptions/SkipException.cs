using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class SkipException : CommandExecutionException
    {
        public override string Title => "Skip";
        public override DiscordColor ExecutedColor => DiscordColor.Blue;

        public SkipException() : base() { }
        public SkipException(string message) : base(message) { }
        public SkipException(string message, Exception innerException) : base(message, innerException) { }
    }
}
