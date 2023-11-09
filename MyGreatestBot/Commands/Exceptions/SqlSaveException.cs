using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class SqlSaveException : CommandExecutionException
    {
        public override string Title => "Save";
        public override DiscordColor ExecutedColor => DiscordColor.Blurple;

        public SqlSaveException() : base() { }
        public SqlSaveException(string message) : base(message) { }
        public SqlSaveException(string message, Exception innerException) : base(message, innerException) { }
    }
}
