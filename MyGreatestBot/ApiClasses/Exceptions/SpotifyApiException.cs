using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Exceptions
{
    public sealed class SpotifyApiException : ApiException
    {
        public SpotifyApiException(
            [DisallowNull] string message,
            [AllowNull] Exception inner = null)
            : base(ApiIntents.Spotify, message, inner) { }

        public SpotifyApiException() : this("Not initialized") { }
    }
}
