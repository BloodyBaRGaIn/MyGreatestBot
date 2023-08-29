using System;

namespace MyGreatestBot.ApiClasses
{
    internal class GenericApiException : Exception
    {
        public GenericApiException()
        {
            throw new NotImplementedException();
        }

        public GenericApiException(ApiIntents intents, string? message = "", Exception? inner = null)
            : base($"{intents} failed. {message ?? ""}", inner)
        {

        }

        public GenericApiException(ApiIntents intents)
            : this(intents, "Not initialized")
        {

        }
    }

    internal sealed class YoutubeApiException : GenericApiException
    {
        public YoutubeApiException(string message, Exception? inner = null)
            : base(ApiIntents.Youtube, message, inner)
        {

        }

        public YoutubeApiException()
            : this("Not initialized")
        {

        }
    }

    internal sealed class YandexApiException : GenericApiException
    {
        public YandexApiException(string message, Exception? inner = null)
            : base(ApiIntents.Yandex, message, inner)
        {

        }

        public YandexApiException()
            : this("Not initialized")
        {

        }
    }

    internal sealed class VkApiException : GenericApiException
    {
        public VkApiException(string message, Exception? inner = null)
            : base(ApiIntents.Vk, message, inner)
        {

        }

        public VkApiException()
            : this("Not initialized")
        {

        }
    }

    internal sealed class SpotifyApiException : GenericApiException
    {
        public SpotifyApiException(string message, Exception? inner = null)
            : base(ApiIntents.Spotify, message, inner)
        {

        }

        public SpotifyApiException()
            : this("Not initialized")
        {

        }
    }

    internal sealed class SqlApiException : GenericApiException
    {
        public SqlApiException(string message, Exception? inner = null)
            : base(ApiIntents.Sql, message, inner)
        {

        }

        public SqlApiException()
            : this("Not initialized")
        {

        }
    }
}
