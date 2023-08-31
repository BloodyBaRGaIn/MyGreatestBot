using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    internal class CommandExecutionException : Exception
    {
        internal virtual string Title => "Command error";
        internal virtual DiscordColor Color => DiscordColor.Red;

        public CommandExecutionException() : base("Exception was thrown") { }
        public CommandExecutionException(string message) : base(message) { }
        public CommandExecutionException(string message, Exception exception) : base(message, exception) { }
    }
}
