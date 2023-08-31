using System;

namespace MyGreatestBot.Commands.Exceptions
{
    internal class IgnoreException : CommandExecutionException
    {
        internal override string Title => "Ignore";
        public IgnoreException() : base() { }
        public IgnoreException(string message) : base(message) { }
        public IgnoreException(string message, Exception innerException) : base(message, innerException) { }
    }
}
