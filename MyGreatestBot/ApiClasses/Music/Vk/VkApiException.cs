using System;

namespace MyGreatestBot.ApiClasses.Music.Vk
{
    public sealed class VkApiException(
        string message,
        Exception? inner = null) : ApiException(ApiIntents.Vk, message, inner)
    {
        public VkApiException() : this(DefaultMessage) { }
    }
}
