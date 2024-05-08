using System;

namespace MyGreatestBot.ApiClasses
{
    public class ApiException : Exception
    {
        protected ApiException(
            ApiIntents intents,
            [DisallowNull] string message = "",
            [AllowNull] Exception inner = null)
            : base($"{intents} failed. {message ?? ""}", inner)
        {

        }

        public ApiException(ApiIntents intents)
            : this(intents, "Not initialized")
        {

        }
    }
}
