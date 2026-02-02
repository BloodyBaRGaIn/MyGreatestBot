using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class BeepCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Beep";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blue;
        public BeepCommandException(string message) : base(message) { }
        public BeepCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
