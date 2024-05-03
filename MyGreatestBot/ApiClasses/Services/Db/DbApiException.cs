using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Services.Db
{
    public sealed class DbApiException(
        [DisallowNull] string message,
        [AllowNull] Exception inner = null) : ApiException(ApiIntents.Db, message, inner)
    {
        public DbApiException() : this("Not initialized") { }
    }
}
