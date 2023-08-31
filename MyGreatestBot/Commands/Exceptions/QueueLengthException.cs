using System;

namespace MyGreatestBot.Commands.Exceptions
{
    internal class QueueLengthException : CommandExecutionException
    {
        internal override string Title => "Count";
        public QueueLengthException() : base() { }
        public QueueLengthException(string message) : base(message) { }
        public QueueLengthException(string message, Exception innerException) : base(message, innerException) { }
    }
}
