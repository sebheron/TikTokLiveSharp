using Newtonsoft.Json.Linq;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TikTokLiveSharp.Client.Proxy;
using TikTokLiveSharp.Protobuf;

namespace TikTokLiveSharp.Client
{
    internal class TikTokHTTPClient
    {
        private const string TIKTOK_URL_WEB = "https://www.tiktok.com/";
        private const string TIKTOK_URL_WEBCAST = "https://webcast.tiktok.com/webcast/";

        private static readonly Dictionary<string, string> DEFAULT_REQUEST_HEADERS = new Dictionary<string, string>()
        {
            { "Connection", "keep-alive"},
            { "Cache-Control", "max-age=0"},
            { "Accept", "text/html,application/json,application/protobuf"},
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML,like Gecko) Chrome/97.0.4692.99 Safari/537.36"},
            { "Referer", "https,//www.tiktok.com/"},
            { "Origin", "https,//www.tiktok.com"},
            { "Accept-Language", "en-US,en; q=0.9"},
            { "Accept-Encoding", "gzip,deflate"}
        };

        private readonly HttpClient client;
        private readonly HttpClientHandler handler;
        private readonly ProxyContainer proxyContainer;

        public TikTokHTTPClient(TimeSpan? timeout, ProxyContainer proxyContainer = null, Dictionary<string, string> additionalHeaders = null)
        {
            this.handler = new HttpClientHandler();
            this.client = new HttpClient(this.handler);
            this.client.Timeout = timeout ?? TimeSpan.FromMilliseconds(10);
            this.client.DefaultRequestHeaders.Clear();
            foreach (var header in DEFAULT_REQUEST_HEADERS)
            {
                this.client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            for (int i = 0; i < (additionalHeaders?.Count ?? 0); i++)
            {
                var vals = additionalHeaders.ElementAt(i);
                this.client.DefaultRequestHeaders.Add(vals.Key, vals.Value);
            }
            this.proxyContainer = proxyContainer ?? new ProxyContainer(false);
        }

        private async Task<byte[]> GetRequest(string url, Dictionary<string, object> parameters = null)
        {
            this.handler.Proxy = new WebProxy(this.proxyContainer.Get());
            var request = await this.client.GetAsync($"{url}?{this.BuildQueryString(parameters ?? new Dictionary<string, object>())}");
            return await request.Content.ReadAsByteArrayAsync();
        }

        private async Task<string> PostRequest(string url, string data, Dictionary<string, object> parameters = null)
        {
            this.handler.Proxy = new WebProxy(this.proxyContainer.Get());
            var request = await this.client.PostAsync($"{url}?{this.BuildQueryString(parameters ?? new Dictionary<string, object>())}",
                new StringContent(data, Encoding.UTF8));
            return await request.Content.ReadAsStringAsync();
        }

        protected async Task<string> GetLivestreamPage(string userID)
        {
            var bytes = await this.GetRequest($"{TIKTOK_URL_WEB}@{userID}/live");
            return Encoding.UTF8.GetString(bytes);
        }

        private async Task<WebcastResponse> GetDeserializedMessage(string path, Dictionary<string, object> parameters)
        {
            var bytes = await this.GetRequest(TIKTOK_URL_WEBCAST + path, parameters);
            return Serializer.Deserialize<WebcastResponse>(new ReadOnlyMemory<byte>(bytes));
        }

        protected async Task<JObject> GetJObjectFromWebcastAPI(string path, Dictionary<string, object> parameters)
        {
            var bytes = await this.GetRequest(TIKTOK_URL_WEBCAST + path, parameters);
            var json = Encoding.UTF8.GetString(bytes);
            return JObject.Parse(json);
        }

        protected async Task<JObject> PostJObjecttToWebcastAPI(string path, Dictionary<string, object> parameters, JObject json)
        {
            var replyJson = await this.PostRequest(TIKTOK_URL_WEBCAST + path, json.ToString(Newtonsoft.Json.Formatting.None), parameters);
            return JObject.Parse(replyJson);
        }

        private string BuildQueryString(Dictionary<string, object> parameters)
        {
            var queryString = new StringBuilder();
            foreach (var key in parameters.Keys)
            {
                queryString.Append(key);
                queryString.Append("=");
                queryString.Append(parameters[key]);
                queryString.Append("&");
            }
            return queryString.ToString();
        }
    }
}
