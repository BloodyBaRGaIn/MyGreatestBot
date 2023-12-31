﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Music.Yandex
{
    public sealed class YandexApiException : ApiException
    {
        public YandexApiException(
            [DisallowNull] string message,
            [AllowNull] Exception inner = null)
            : base(ApiIntents.Yandex, message, inner) { }

        public YandexApiException() : this("Not initialized") { }
    }
}
