using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace TikTokLiveSharp.Client.Requests
{
    public interface ITikTokHttpRequest
    {
        /// <summary>
        /// Sends an async get request.
        /// </summary>
        /// <returns>HttpContent returned.</returns>
        /// <exception cref="Exception">Requests should not be reused.</exception>
        Task<HttpContent> Get();

        /// <summary>
        /// Sends an async post request.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Requests should not be reused.</exception>
        Task<HttpContent> Post(HttpContent content);

        /// <summary>
        /// Sets the queries for the request.
        /// </summary>
        /// <param name="queries">The queries to append to the URL.</param>
        /// <returns>Request with queries added.</returns>
        ITikTokHttpRequest SetQueries(IDictionary<string, object> queries);
    }
}