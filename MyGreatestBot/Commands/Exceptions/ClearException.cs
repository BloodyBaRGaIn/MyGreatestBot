using System;

namespace MyGreatestBot.Commands.Exceptions
{
    internal class ClearException : CommandExecutionException
    {
        internal override string Title => "Clear";
        public ClearException() : base() { }
        public ClearException(string message) : base(message) { }
        public ClearException(string message, Exception innerException) : base(message, innerException) { }
    }
}
