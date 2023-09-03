using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ReturnException : CommandExecutionException
    {
        public override string Title => "Return";
        public override DiscordColor ExecutedColor => DiscordColor.Yellow;

        public ReturnException() : base() { }
        public ReturnException(string message) : base(message) { }
        public ReturnException(string message, Exception innerException) : base(message, innerException) { }
    }
}
