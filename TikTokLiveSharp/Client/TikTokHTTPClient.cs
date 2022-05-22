using Newtonsoft.Json.Linq;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TikTokLiveSharp.Client.Proxy;
using TikTokLiveSharp.Client.Requests;
using TikTokLiveSharp.Protobuf;

namespace TikTokLiveSharp.Client
{
    public class TikTokHTTPClient
    {
        internal TikTokHTTPClient(TimeSpan? timeout, RotatingProxy proxyHandler = null)
        {
            TikTokHttpRequest.Timeout = timeout ?? TimeSpan.FromSeconds(2);
            TikTokHttpRequest.WebProxy = proxyHandler;
        }

        private ITikTokHttpRequest BuildRequest(string url, Dictionary<string, object> parameters = null)
        {
            return new TikTokHttpRequest(url)
                .SetQueries(parameters);
        }

        private async Task<HttpContent> GetRequest(string url, Dictionary<string, object> parameters = null)
        {
            var request = this.BuildRequest(url, parameters);
            return await request.Get();
        }

        private async Task<HttpContent> PostRequest(string url, string data, Dictionary<string, object> parameters = null)
        {
            var request = this.BuildRequest(url, parameters);
            return await request.Post(new StringContent(data, Encoding.UTF8));
        }

        internal async Task<string> GetLivestreamPage(string uniqueID)
        {
            var get = await this.GetRequest($"{TikTokRequestSettings.TIKTOK_URL_WEB}@{uniqueID}/live/");
            return await get.ReadAsStringAsync();
        }

        internal async Task<WebcastResponse> GetDeserializedMessage(string path, Dictionary<string, object> parameters)
        {
            var get = await this.GetRequest(TikTokRequestSettings.TIKTOK_URL_WEBCAST + path, parameters);
            return Serializer.Deserialize<WebcastResponse>(await get.ReadAsStreamAsync());
        }

        internal async Task<JObject> GetJObjectFromWebcastAPI(string path, Dictionary<string, object> parameters)
        {
            var get = await this.GetRequest(TikTokRequestSettings.TIKTOK_URL_WEBCAST + path, parameters);
            return JObject.Parse(await get.ReadAsStringAsync());
        }

        internal async Task<JObject> PostJObjecttToWebcastAPI(string path, Dictionary<string, object> parameters, JObject json)
        {
            var post = await this.PostRequest(TikTokRequestSettings.TIKTOK_URL_WEBCAST + path, json.ToString(Newtonsoft.Json.Formatting.None), parameters);
            return JObject.Parse(await post.ReadAsStringAsync());
        }
    }
}
