using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class SqlRestoreException : CommandExecutionException
    {
        public override string Title => "Restore";
        public override DiscordColor ExecutedColor => DiscordColor.Blurple;

        public SqlRestoreException() : base() { }
        public SqlRestoreException(string message) : base(message) { }
        public SqlRestoreException(string message, Exception innerException) : base(message, innerException) { }
    }
}
