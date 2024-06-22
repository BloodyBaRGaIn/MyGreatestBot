using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ResumeException : CommandExecutionException
    {
        public override string Title { get; } = "Resume";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Green;
        public ResumeException(string message) : base(message) { }
        public ResumeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
