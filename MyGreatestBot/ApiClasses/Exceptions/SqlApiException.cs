﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Exceptions
{
    public sealed class SqlApiException : ApiException
    {
        public SqlApiException(
            [DisallowNull] string message,
            [AllowNull] Exception inner = null)
            : base(ApiIntents.Sql, message, inner) { }

        public SqlApiException() : this("Not initialized") { }
    }
}
