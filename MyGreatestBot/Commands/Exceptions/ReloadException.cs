using System;

namespace MyGreatestBot.Commands.Exceptions
{
    internal class ReloadException : CommandExecutionException
    {
        internal override string Title => "Reload";
        public ReloadException() : base() { }
        public ReloadException(string message) : base(message) { }
        public ReloadException(string message, Exception innerException) : base(message, innerException) { }
    }
}
