﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Music.Youtube
{
    public sealed class YoutubeApiException : ApiException
    {
        public YoutubeApiException(
            [DisallowNull] string message,
            [AllowNull] Exception inner = null)
            : base(ApiIntents.Youtube, message, inner) { }

        public YoutubeApiException() : this("Not initialized") { }
    }
}
