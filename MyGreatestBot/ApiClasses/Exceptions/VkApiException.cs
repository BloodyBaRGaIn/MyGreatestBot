using System;

namespace MyGreatestBot.ApiClasses.Exceptions
{
    internal sealed class VkApiException : ApiException
    {
        public VkApiException(string message, Exception? inner = null)
            : base(ApiIntents.Vk, message, inner) { }

        public VkApiException() : this("Not initialized") { }
    }
}
