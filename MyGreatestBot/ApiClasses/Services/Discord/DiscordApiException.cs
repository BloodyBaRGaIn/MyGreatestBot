using System;

namespace MyGreatestBot.ApiClasses.Services.Discord
{
    public sealed class DiscordApiException(
        [DisallowNull] string message,
        [AllowNull] Exception inner = null) : ApiException(ApiIntents.Discord, message, inner)
    {
        public DiscordApiException() : this("Not initialized") { }
    }
}
