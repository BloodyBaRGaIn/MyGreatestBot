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

    public sealed class RestoreException : CommandExecutionException
    {
        public override string Title => "Restore";
        public override DiscordColor ExecutedColor => DiscordColor.Blurple;

        public RestoreException() : base() { }
        public RestoreException(string message) : base(message) { }
        public RestoreException(string message, Exception innerException) : base(message, innerException) { }
    }
}
