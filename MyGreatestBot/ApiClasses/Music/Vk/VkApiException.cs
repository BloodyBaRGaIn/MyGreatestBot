using System;

namespace MyGreatestBot.ApiClasses.Music.Vk
{
    public sealed class VkApiException(
        [DisallowNull] string message,
        [AllowNull] Exception inner = null) : ApiException(ApiIntents.Vk, message, inner)
    {
        public VkApiException() : this("Not initialized") { }
    }
}
