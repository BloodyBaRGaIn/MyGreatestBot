﻿using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class SqlRestoreException : CommandExecutionException
    {
        public override string Title { get; } = "Restore";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blurple;

        public SqlRestoreException() : base() { }
        public SqlRestoreException(string message) : base(message) { }
        public SqlRestoreException(string message, Exception innerException) : base(message, innerException) { }
    }
}
