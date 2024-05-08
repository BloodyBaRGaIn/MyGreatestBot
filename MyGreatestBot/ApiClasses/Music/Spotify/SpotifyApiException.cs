using System;

namespace MyGreatestBot.ApiClasses.Music.Spotify
{
    public sealed class SpotifyApiException(
        [DisallowNull] string message,
        [AllowNull] Exception inner = null) : ApiException(ApiIntents.Spotify, message, inner)
    {
        public SpotifyApiException() : this("Not initialized") { }
    }
}
