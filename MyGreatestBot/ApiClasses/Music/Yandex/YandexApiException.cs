using System;

namespace MyGreatestBot.ApiClasses.Music.Yandex
{
    public sealed class YandexApiException(
        [DisallowNull] string message,
        [AllowNull] Exception inner = null) : ApiException(ApiIntents.Yandex, message, inner)
    {
        public YandexApiException() : this("Not initialized") { }
    }
}
