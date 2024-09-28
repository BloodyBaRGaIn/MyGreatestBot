using System;

namespace MyGreatestBot.ApiClasses
{
    public class ApiException : Exception
    {
        protected const string DefaultMessage = "Not initialized";

        protected ApiException(
            ApiIntents intents,
            string? message = null,
            Exception? inner = null)
            : base($"{intents} failed. {message ?? string.Empty}", inner)
        {

        }

        public ApiException(ApiIntents intents) : this(intents, DefaultMessage)
        {

        }
    }
}
