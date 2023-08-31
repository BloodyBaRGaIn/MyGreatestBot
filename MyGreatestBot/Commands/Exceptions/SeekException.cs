using System;

namespace MyGreatestBot.Commands.Exceptions
{
    internal class SeekException : CommandExecutionException
    {
        internal override string Title => "Seek";
        public SeekException() : base() { }
        public SeekException(string message) : base(message) { }
        public SeekException(string message, Exception innerException) : base(message, innerException) { }
    }
}
