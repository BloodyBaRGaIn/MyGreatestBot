using MyGreatestBot.ApiClasses.Utils;
using System;
using System.Net.Http;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// Interface for API domains
    /// </summary>
    public interface IAccessible : IAPI
    {
        /// <summary>
        /// Collection of API domain URLs
        /// </summary>
        DomainCollection Domains { get; }

        /// <summary>
        /// Try HTTP request to domain URLs.<br/>
        /// Throws <see cref="ApplicationException"/> if fails.
        /// </summary>
        /// 
        /// <exception cref="ApplicationException"></exception>
        public void TryAccess()
        {
            if (Domains == null)
            {
                return;
            }

            foreach (string url in Domains)
            {
                if (IsUrlSuccess(url))
                {
                    return;
                }
            }

            throw new ApplicationException($"{ApiType} is not available");
        }

        /// <summary>
        /// Checks if given URL is available.
        /// </summary>
        /// 
        /// <param name="url">
        /// URL to check.
        /// </param>
        /// 
        /// <param name="isApi">
        /// True if <see cref="HttpMethod.Get"/> is required,<br/>
        /// otherwise <see cref="HttpMethod.Head"/> will be performed.
        /// </param>
        /// 
        /// <returns>
        /// True if the response returned successful status code or given URL is an empty string.
        /// </returns>
        public static bool IsUrlSuccess(
            [DisallowNull] string url,
            bool isApi = true)
        {
            if (url == string.Empty)
            {
                // bypass accessing check
                return true;
            }
            using HttpClient client = new();
            try
            {
                using HttpResponseMessage message = client.Send(
                    new(isApi ? HttpMethod.Get : HttpMethod.Head,
                        url));

                if (message.IsSuccessStatusCode || (int)message.StatusCode == 418)
                {
                    return true;
                }
            }
            catch { }

            return false;
        }
    }
}
