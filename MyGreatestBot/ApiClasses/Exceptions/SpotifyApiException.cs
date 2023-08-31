using System;

namespace MyGreatestBot.ApiClasses.Exceptions
{
    internal sealed class SpotifyApiException : ApiException
    {
        public SpotifyApiException(string message, Exception? inner = null)
            : base(ApiIntents.Spotify, message, inner) { }

        public SpotifyApiException() : this("Not initialized") { }
    }
}
