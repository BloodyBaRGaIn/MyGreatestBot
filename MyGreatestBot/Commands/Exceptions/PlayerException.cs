using System;

namespace MyGreatestBot.Commands.Exceptions
{
    internal class PlayerException : CommandExecutionException
    {
        internal override string Title => "Player";
        public PlayerException() : base() { }
        public PlayerException(string message) : base(message) { }
        public PlayerException(string message, Exception innerException) : base(message, innerException) { }
    }
}
