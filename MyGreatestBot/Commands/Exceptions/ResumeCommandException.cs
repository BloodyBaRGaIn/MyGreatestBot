using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ResumeCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Resume";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Green;
        public ResumeCommandException(string message) : base(message) { }
        public ResumeCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
