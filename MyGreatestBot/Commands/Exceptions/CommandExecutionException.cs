using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public class CommandExecutionException : Exception
    {
        public virtual string Title => "Command error";
        public virtual DiscordColor ErroredColor => DiscordColor.Red;
        public virtual DiscordColor ExecutedColor => DiscordColor.White;

        public CommandExecutionException() : base("Exception was thrown") { }
        public CommandExecutionException(string message) : base(message) { }
        public CommandExecutionException(string message, Exception exception) : base(message, exception) { }
    }
}
