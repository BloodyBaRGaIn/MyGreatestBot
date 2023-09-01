﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Exceptions
{
    public sealed class VkApiException : ApiException
    {
        public VkApiException(
            [DisallowNull] string message,
            [AllowNull] Exception inner = null) 
            : base(ApiIntents.Vk, message, inner) { }

        public VkApiException() : this("Not initialized") { }
    }
}
