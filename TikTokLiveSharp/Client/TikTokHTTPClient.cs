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
using Flurl.Http;
using Flurl;

namespace TikTokLiveSharp.Client
{
    public class TikTokHTTPClient
    {
        private const string TIKTOK_URL_WEB = "https://www.tiktok.com/";
        private const string TIKTOK_URL_WEBCAST = "https://webcast.tiktok.com/webcast/";

        private static readonly Dictionary<string, string> DEFAULT_REQUEST_HEADERS = new Dictionary<string, string>()
        {
            { "Connection", "keep-alive" },
            { "Cache-Control", "max-age=0" },
            { "Accept", "text/html,application/json,application/protobuf" },
            { "User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Mobile Safari/537.36" },
            { "Referer", "https,//www.tiktok.com/" },
            { "Origin", "https,//www.tiktok.com" },
            { "Accept-Language", "en-US,en; q=0.9" },
            { "Accept-Encoding", "gzip, deflate" },
            { "Host", "www.tiktok.com" }
        };

        public ProxyContainer proxyContainer { get; }

        public TimeSpan Timeout { get; }

        internal TikTokHTTPClient(TimeSpan? timeout, ProxyContainer proxyContainer = null, Dictionary<string, string> additionalHeaders = null)
        {
            this.Timeout = timeout ?? TimeSpan.FromSeconds(1);
            this.proxyContainer = proxyContainer ?? new ProxyContainer(false);
        }

        private IFlurlRequest BuildFlurlRequest(string url, Dictionary<string, object> parameters = null)
        {
            var request = url.WithHeaders(DEFAULT_REQUEST_HEADERS)
                .WithTimeout(this.Timeout);
            for (int i = 0; i < (parameters?.Count ?? 0); i++)
            {
                var vals = parameters.ElementAt(i);
                request.SetQueryParam(vals.Key, vals.Value);
            }
            return request;
        }

        private async Task<IFlurlResponse> GetRequest(string url, Dictionary<string, object> parameters = null)
        {
            //this.handler.Proxy = new WebProxy(this.proxyContainer.Get());
            var request = this.BuildFlurlRequest(url, parameters);
            return await request.GetAsync();
        }

        private async Task<IFlurlResponse> PostRequest(string url, string data, Dictionary<string, object> parameters = null)
        {
            //this.handler.Proxy = new WebProxy(this.proxyContainer.Get());
            var request = this.BuildFlurlRequest(url, parameters);
            return await request.PostAsync(new StringContent(data, Encoding.UTF8));
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

        internal async Task<string> GetLivestreamPage(string userID)
        {
            return await this.GetRequest($"{TIKTOK_URL_WEB}@{userID}/live").ReceiveString();
        }

        internal async Task<WebcastResponse> GetDeserializedMessage(string path, Dictionary<string, object> parameters)
        {
            var bytes = await this.GetRequest(TIKTOK_URL_WEBCAST + path, parameters).ReceiveStream();
            return Serializer.Deserialize<WebcastResponse>(bytes);
        }

        internal async Task<JObject> GetJObjectFromWebcastAPI(string path, Dictionary<string, object> parameters)
        {
            var json = await this.GetRequest(TIKTOK_URL_WEBCAST + path, parameters).ReceiveString();
            return JObject.Parse(json);
        }

        internal async Task<JObject> PostJObjecttToWebcastAPI(string path, Dictionary<string, object> parameters, JObject json)
        {
            var replyJson = await this.PostRequest(TIKTOK_URL_WEBCAST + path, json.ToString(Newtonsoft.Json.Formatting.None), parameters).ReceiveString();
            return JObject.Parse(replyJson);
        }
    }
}
