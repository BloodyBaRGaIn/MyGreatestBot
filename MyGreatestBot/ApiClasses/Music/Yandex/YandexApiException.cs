using System;

namespace MyGreatestBot.ApiClasses.Music.Yandex
{
    public sealed class YandexApiException(
        string message,
        Exception? inner = null) : ApiException(ApiIntents.Yandex, message, inner)
    {
        public YandexApiException() : this(DefaultMessage) { }
    }
}
