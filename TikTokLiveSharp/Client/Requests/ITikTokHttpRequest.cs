using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace TikTokLiveSharp.Client.Requests
{
    public interface ITikTokHttpRequest
    {
        Task<HttpContent> Get();

        Task<HttpContent> Post(HttpContent content);

        ITikTokHttpRequest SetQueries(IDictionary<string, object> queries);
    }
}