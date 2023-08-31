using System;

namespace MyGreatestBot.Commands.Exceptions
{
    internal class SkipException : CommandExecutionException
    {
        internal override string Title => "Skip";
        public SkipException() : base() { }
        public SkipException(string message) : base(message) { }
        public SkipException(string message, Exception innerException) : base(message, innerException) { }
    }
}
