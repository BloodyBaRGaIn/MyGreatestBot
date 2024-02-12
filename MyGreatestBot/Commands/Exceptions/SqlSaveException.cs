using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class SqlSaveException : CommandExecutionException
    {
        public override string Title { get; } = "Save";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blurple;

        public SqlSaveException() : base() { }
        public SqlSaveException(string message) : base(message) { }
        public SqlSaveException(string message, Exception innerException) : base(message, innerException) { }
    }
}
