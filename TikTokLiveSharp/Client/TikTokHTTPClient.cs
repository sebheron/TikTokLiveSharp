using Newtonsoft.Json.Linq;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TikTokLiveSharp.Client.Proxy;
using TikTokLiveSharp.Protobuf;
using Flurl.Http;
using Flurl;
using Flurl.Http.Configuration;

namespace TikTokLiveSharp.Client
{
    public class TikTokHTTPClient
    {
        public TimeSpan Timeout { get; }

        internal TikTokHTTPClient(TimeSpan? timeout, ProxyClientFactory proxyClientFactory = null)
        {
            this.Timeout = timeout ?? TimeSpan.FromSeconds(2);
            FlurlHttp.Configure(settings => {
                settings.HttpClientFactory = proxyClientFactory ?? new DefaultHttpClientFactory();
            });
        }

        private IFlurlRequest BuildFlurlRequest(string url, Dictionary<string, object> parameters = null)
        {
            var request = url.WithHeaders(TikTokRequestSettings.DEFAULT_REQUEST_HEADERS)
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
            var request = this.BuildFlurlRequest(url, parameters);
            return await request.GetAsync();
        }

        private async Task<IFlurlResponse> PostRequest(string url, string data, Dictionary<string, object> parameters = null)
        {
            var request = this.BuildFlurlRequest(url, parameters);
            return await request.PostAsync(new StringContent(data, Encoding.UTF8));
        }

        internal async Task<string> GetLivestreamPage(string uniqueID)
        {
            return await this.GetRequest($"{TikTokRequestSettings.TIKTOK_URL_WEB}@{uniqueID}/live/").ReceiveString();
        }

        internal async Task<WebcastResponse> GetDeserializedMessage(string path, Dictionary<string, object> parameters)
        {
            var bytes = await this.GetRequest(TikTokRequestSettings.TIKTOK_URL_WEBCAST + path, parameters).ReceiveStream();
            return Serializer.Deserialize<WebcastResponse>(bytes);
        }

        internal async Task<JObject> GetJObjectFromWebcastAPI(string path, Dictionary<string, object> parameters)
        {
            var json = await this.GetRequest(TikTokRequestSettings.TIKTOK_URL_WEBCAST + path, parameters).ReceiveString();
            return JObject.Parse(json);
        }

        internal async Task<JObject> PostJObjecttToWebcastAPI(string path, Dictionary<string, object> parameters, JObject json)
        {
            var replyJson = await this.PostRequest(TikTokRequestSettings.TIKTOK_URL_WEBCAST + path, json.ToString(Newtonsoft.Json.Formatting.None), parameters).ReceiveString();
            return JObject.Parse(replyJson);
        }
    }
}
