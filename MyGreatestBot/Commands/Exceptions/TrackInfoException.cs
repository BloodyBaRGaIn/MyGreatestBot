using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class TrackInfoException : CommandExecutionException
    {
        public override string Title => "Track info";
        public override DiscordColor ExecutedColor => GenericColor;

        public TrackInfoException() : base() { }
        public TrackInfoException(string message) : base(message) { }
        public TrackInfoException(string message, Exception innerException) : base(message, innerException) { }
    }
}
