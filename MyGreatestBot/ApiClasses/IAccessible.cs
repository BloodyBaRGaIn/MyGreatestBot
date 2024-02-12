using MyGreatestBot.ApiClasses.Utils;
using System;
using System.Net.Http;

using DisallowNullAttribute = System.Diagnostics.CodeAnalysis.DisallowNullAttribute;

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
        /// Try HTTP request to domain URLs
        /// </summary>
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

        public static bool IsUrlSuccess([DisallowNull] string url, bool isApi = true)
        {
            if (url == string.Empty)
            {
                // bypass accessing check
                return true;
            }
            using HttpClient client = new();
            try
            {
                using HttpResponseMessage message = client.Send(new(isApi ? HttpMethod.Get : HttpMethod.Head,
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
