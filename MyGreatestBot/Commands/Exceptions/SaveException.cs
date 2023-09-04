using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class SaveException : CommandExecutionException
    {
        public override string Title => "Save";
        public override DiscordColor ExecutedColor => DiscordColor.Blurple;

        public SaveException() : base() { }
        public SaveException(string message) : base(message) { }
        public SaveException(string message, Exception innerException) : base(message, innerException) { }
    }
}
