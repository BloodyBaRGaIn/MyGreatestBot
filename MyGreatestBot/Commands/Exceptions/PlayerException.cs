﻿using DSharpPlus.Entities;
using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class PlayerException : CommandExecutionException
    {
        public override string Title => "Player";
        public override DiscordColor ExecutedColor => GenericColor;

        public PlayerException() : base() { }
        public PlayerException(string message) : base(message) { }
        public PlayerException(string message, Exception innerException) : base(message, innerException) { }
    }
}
