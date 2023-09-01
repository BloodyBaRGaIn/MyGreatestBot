using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public class IgnoreException : CommandExecutionException
    {
        public override string Title => "Ignore";
        public override DiscordColor ExecutedColor => DiscordColor.Yellow;

        public IgnoreException() : base() { }
        public IgnoreException(string message) : base(message) { }
        public IgnoreException(string message, Exception innerException) : base(message, innerException) { }
    }
}
