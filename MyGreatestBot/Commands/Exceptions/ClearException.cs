using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ClearException : CommandExecutionException
    {
        public override string Title { get; } = "Clear";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Aquamarine;

        public ClearException() : base() { }
        public ClearException(string message) : base(message) { }
        public ClearException(string message, Exception innerException) : base(message, innerException) { }
    }
}
