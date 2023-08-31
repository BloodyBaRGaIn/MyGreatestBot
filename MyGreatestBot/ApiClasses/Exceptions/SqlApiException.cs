using System;

namespace MyGreatestBot.ApiClasses.Exceptions
{
    public sealed class SqlApiException : ApiException
    {
        public SqlApiException(string message, Exception? inner = null)
            : base(ApiIntents.Sql, message, inner) { }

        public SqlApiException() : this("Not initialized") { }
    }
}
