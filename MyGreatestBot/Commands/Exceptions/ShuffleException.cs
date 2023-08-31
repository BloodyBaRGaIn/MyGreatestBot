using System;

namespace MyGreatestBot.Commands.Exceptions
{
    internal class ShuffleException : CommandExecutionException
    {
        internal override string Title => "Shuffle";
        public ShuffleException() : base() { }
        public ShuffleException(string message) : base(message) { }
        public ShuffleException(string message, Exception innerException) : base(message, innerException) { }
    }
}
