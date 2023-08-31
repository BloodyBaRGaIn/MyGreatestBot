using System;

namespace MyGreatestBot.Commands.Exceptions
{
    internal class TrackInfoException : CommandExecutionException
    {
        internal override string Title => "Track info";
        public TrackInfoException() : base() { }
        public TrackInfoException(string message) : base(message) { }
        public TrackInfoException(string message, Exception innerException) : base(message, innerException) { }
    }
}
