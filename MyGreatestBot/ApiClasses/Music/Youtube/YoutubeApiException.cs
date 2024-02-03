using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Music.Youtube
{
    public sealed class YoutubeApiException(
        [DisallowNull] string message,
        [AllowNull] Exception inner = null) : ApiException(ApiIntents.Youtube, message, inner)
    {
        public YoutubeApiException() : this("Not initialized") { }
    }
}
