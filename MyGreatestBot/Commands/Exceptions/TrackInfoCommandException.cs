using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class TrackInfoCommandException : CommandExecutionException
    {
        public override string Title { get; } = "Track info";
        protected override DiscordColor ExecutedColor { get; } = GenericColor;
        public TrackInfoCommandException(string message) : base(message) { }
        public TrackInfoCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
