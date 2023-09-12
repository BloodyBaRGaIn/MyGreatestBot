using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Services.Discord
{
    public sealed class DiscordApiException : ApiException
    {
        public DiscordApiException(
            [DisallowNull] string message,
            [AllowNull] Exception inner = null)
            : base(ApiIntents.Sql, message, inner) { }

        public DiscordApiException() : this("Not initialized") { }
    }
}
