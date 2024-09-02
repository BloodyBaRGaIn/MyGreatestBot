using System;

namespace MyGreatestBot.Commands.Exceptions
{
    public sealed class ApiStatusCommandException : CommandExecutionException
    {
        public override string Title { get; } = "API Status";
        protected override DiscordColor ExecutedColor { get; } = DiscordColor.Blue;
        public ApiStatusCommandException(string message) : base(message) { }
        public ApiStatusCommandException(string message, Exception innerException) : base(message, innerException) { }
    }
}
