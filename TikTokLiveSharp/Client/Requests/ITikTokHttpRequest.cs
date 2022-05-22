using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TikTokLiveSharp.Client.Requests
{
    public interface ITikTokHttpRequest
    {
        Task<HttpContent> Post(HttpContent content);
        Task<HttpContent> Get();

        ITikTokHttpRequest SetQueries(IDictionary<string, object> queries);
    }
}
