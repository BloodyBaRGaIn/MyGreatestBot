﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Exceptions
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