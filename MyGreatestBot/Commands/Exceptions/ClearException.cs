using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ClearException : CommandExecutionException
    {
        public override string Title => "Clear";
        public override DiscordColor ExecutedColor => DiscordColor.Aquamarine;

        public ClearException() : base() { }
        public ClearException(string message) : base(message) { }
        public ClearException(string message, Exception innerException) : base(message, innerException) { }
    }
}
