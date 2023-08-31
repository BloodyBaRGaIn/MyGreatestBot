using System;

namespace MyGreatestBot.ApiClasses.Exceptions
{
    internal sealed class YandexApiException : ApiException
    {
        public YandexApiException(string message, Exception? inner = null)
            : base(ApiIntents.Yandex, message, inner) { }

        public YandexApiException() : this("Not initialized") { }
    }
}
