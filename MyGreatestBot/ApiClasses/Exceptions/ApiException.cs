using System;

namespace MyGreatestBot.ApiClasses.Exceptions
{
    internal class ApiException : Exception
    {
        protected ApiException(ApiIntents intents, string? message = "", Exception? inner = null)
            : base($"{intents} failed. {message ?? ""}", inner)
        {

        }

        public ApiException(ApiIntents intents)
            : this(intents, "Not initialized")
        {

        }
    }
}
