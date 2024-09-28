using System;

namespace MyGreatestBot.ApiClasses.Music.Spotify
{
    public sealed class SpotifyApiException(
        string message,
        Exception? inner = null) : ApiException(ApiIntents.Spotify, message, inner)
    {
        public SpotifyApiException() : this(DefaultMessage) { }
    }
}
