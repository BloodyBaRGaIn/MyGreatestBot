using System;

namespace MyGreatestBot.Commands.Exceptions
{
    internal class ReturnException : CommandExecutionException
    {
        internal override string Title => "Return";
        public ReturnException() : base() { }
        public ReturnException(string message) : base(message) { }
        public ReturnException(string message, Exception innerException) : base(message, innerException) { }
    }
}
