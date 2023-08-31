using System;

namespace MyGreatestBot.ApiClasses.Exceptions
{
    internal sealed class YoutubeApiException : ApiException
    {
        public YoutubeApiException(string message, Exception? inner = null)
            : base(ApiIntents.Youtube, message, inner) { }

        public YoutubeApiException() : this("Not initialized") { }
    }
}
