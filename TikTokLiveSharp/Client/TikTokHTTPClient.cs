using Newtonsoft.Json.Linq;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TikTokLiveSharp.Client.Proxy;
using TikTokLiveSharp.Client.Requests;
using TikTokLiveSharp.Errors;
using TikTokLiveSharp.Models;

namespace TikTokLiveSharp.Client
{
    public class TikTokHTTPClient
    {
        private static int uuc = 0;

        internal TikTokHTTPClient(TimeSpan? timeout, RotatingProxy proxyHandler = null)
        {
            TikTokHttpRequest.Timeout = timeout ?? TimeSpan.FromSeconds(15);
            TikTokHttpRequest.WebProxy = proxyHandler;
            uuc++;
        }

        internal async Task<WebcastResponse> GetDeserializedMessage(string path, Dictionary<string, object> parameters, bool signURL = false)
        {
            var get = await this.GetRequest(TikTokRequestSettings.TIKTOK_URL_WEBCAST + path, parameters, signURL);
            return Serializer.Deserialize<WebcastResponse>(await get.ReadAsStreamAsync());
        }

        internal async Task<JObject> GetJObjectFromWebcastAPI(string path, Dictionary<string, object> parameters, bool signURL = false)
        {
            var get = await this.GetRequest(TikTokRequestSettings.TIKTOK_URL_WEBCAST + path, parameters, signURL);
            return JObject.Parse(await get.ReadAsStringAsync());
        }

        internal async Task<string> GetLivestreamPage(string uniqueID, bool signURL = false)
        {
            var get = await this.GetRequest($"{TikTokRequestSettings.TIKTOK_URL_WEB}@{uniqueID}/live/", signURL: signURL);
            return await get.ReadAsStringAsync();
        }

        internal async Task<JObject> PostJObjecttToWebcastAPI(string path, Dictionary<string, object> parameters, JObject json, bool signURL = false)
        {
            var post = await this.PostRequest(TikTokRequestSettings.TIKTOK_URL_WEBCAST + path, json.ToString(Newtonsoft.Json.Formatting.None), parameters, signURL);
            return JObject.Parse(await post.ReadAsStringAsync());
        }

        private async Task<HttpContent> GetRequest(string url, Dictionary<string, object> parameters = null, bool signURL = false)
        {
            var request = this.BuildRequest(signURL ? await this.GetSignedUrl(url, parameters) : url, signURL ? null : parameters);
            return await request.Get();
        }

        private async Task<HttpContent> PostRequest(string url, string data, Dictionary<string, object> parameters = null, bool signURL = false)
        {
            var request = this.BuildRequest(signURL ? await this.GetSignedUrl(url, parameters) : url, signURL ? null : parameters);
            return await request.Post(new StringContent(data, Encoding.UTF8));
        }

        private ITikTokHttpRequest BuildRequest(string url, Dictionary<string, object> parameters = null)
        {
            return new TikTokHttpRequest(url)
                .SetQueries(parameters);
        }
        private async Task<string> GetSignedUrl(string url, Dictionary<string, object> parameters = null)
        {
            var parsedParameters = parameters != null ? "?" + string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}")) : string.Empty;
            var request = new TikTokHttpRequest(TikTokRequestSettings.TIKTOK_SIGN_API)
                .SetQueries(new Dictionary<string, object>()
                {
                    { "client", "ttlive-net" },
                    { "uuc", uuc },
                    { "url", url + parsedParameters }
                });
            var content = await request.Get();
            try
            {
                var json = JObject.Parse(await content.ReadAsStringAsync());
                var signedUrl = json.SelectToken("..signedUrl").Value<string>();
                var userAgent = json.SelectToken("..User-Agent").Value<string>();
                TikTokHttpRequest.CurrentHeaders.Remove("User-Agent");
                TikTokHttpRequest.CurrentHeaders.Add("User-Agent", userAgent);
                return signedUrl;
            }
            catch (Exception e)
            {
                throw new InsuffcientSigningException("Insufficent values have been supplied for signing. Likely due to an update. Post an issue on GitHub.", e);
            }
        }
    }
}