using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ResumeException : CommandExecutionException
    {
        public override string Title => "Resume";
        public override DiscordColor ExecutedColor => DiscordColor.Green;

        public ResumeException() : base() { }
        public ResumeException(string message) : base(message) { }
        public ResumeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
