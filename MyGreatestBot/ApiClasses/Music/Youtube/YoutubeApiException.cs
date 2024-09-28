using System;

namespace MyGreatestBot.ApiClasses.Music.Youtube
{
    public sealed class YoutubeApiException(
        string message,
        Exception? inner = null) : ApiException(ApiIntents.Youtube, message, inner)
    {
        public YoutubeApiException() : this(DefaultMessage) { }
    }
}
