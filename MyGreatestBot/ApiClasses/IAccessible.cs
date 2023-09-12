using MyGreatestBot.ApiClasses.Utils;
using System;
using System.IO;
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
                if (url == string.Empty)
                {
                    // bypass accessing check
                    return;
                }
                HttpClient client = new();
                try
                {
                    HttpResponseMessage message = client.Send(new HttpRequestMessage(HttpMethod.Get, url));

                    using StreamReader stream = new(message.Content.ReadAsStream());
                    string content = stream.ReadToEnd();
                    stream.Close();

                    if (message.IsSuccessStatusCode)
                    {
                        return;
                    }
                }
                catch { }
                finally
                {
                    client.Dispose();
                }
            }

            throw new ApplicationException($"{ApiType} is not available");
        }
    }
}
