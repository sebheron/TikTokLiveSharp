using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace TikTokLiveSharp.Client.Requests
{
    public class TikTokHttpRequest : ITikTokHttpRequest
    {
        private static HttpClient client;
        private static HttpClientHandler handler;
        private static TikTokCookieJar cookieJar;

        private static TimeSpan timeout;
        private static IWebProxy webProxy;

        private string query;
        private HttpRequestMessage request;
        private bool sent;

        /// <summary>
        /// Creates a TikTok http request instance.
        /// </summary>
        /// <param name="url">The url to send to.</param>
        /// <exception cref="ArgumentException">Throws exception if URL is invalid.</exception>
        public TikTokHttpRequest(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri result)) throw new ArgumentException();

            if (cookieJar == null)
            {
                cookieJar = new TikTokCookieJar();
            }
            if (handler == null)
            {
                handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    Proxy = WebProxy,
                    UseProxy = WebProxy == null ? false : true,
                    CookieContainer = new CookieContainer()
                };
            }
            if (client == null)
            {
                client = new HttpClient(handler)
                {
                    Timeout = Timeout
                };
                foreach (var header in TikTokRequestSettings.DEFAULT_REQUEST_HEADERS)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            this.request = new HttpRequestMessage
            {
                RequestUri = result
            };

            this.sent = false;
        }

        /// <summary>
        /// The timeout value used.
        /// </summary>
        public static TimeSpan Timeout
        {
            get => timeout;
            set
            {
                if (client != null) throw new Exception("Timeout cannot be set after client has been initalised.");
                timeout = value;
            }
        }

        /// <summary>
        /// The web proxy used.
        /// </summary>
        public static IWebProxy WebProxy
        {
            get => webProxy;
            set
            {
                if (client != null) throw new Exception("Web proxy cannot be set after client has been initalised.");
                webProxy = value;
            }
        }

        /// <summary>
        /// The cookie jar.
        /// </summary>
        public static TikTokCookieJar CookieJar
        {
            get => cookieJar;
        }

        /// <summary>
        /// The current headers in use.
        /// </summary>
        public static HttpRequestHeaders CurrentHeaders
        {
            get => client.DefaultRequestHeaders;
        }

        /// <summary>
        /// Sends an async get request.
        /// </summary>
        /// <returns>HttpContent returned.</returns>
        /// <exception cref="Exception">Requests should not be reused.</exception>
        public async Task<HttpContent> Get()
        {
            if (this.sent) throw new Exception("Requests should not be reused");
            this.request.Method = HttpMethod.Get;
            return await GetContent();
        }

        /// <summary>
        /// Sends an async post request.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Requests should not be reused.</exception>
        public async Task<HttpContent> Post(HttpContent data)
        {
            if (this.sent) throw new Exception("Requests should not be reused");
            this.request.Method = HttpMethod.Post;
            this.request.Content = data;
            return await GetContent();
        }

        /// <summary>
        /// Sets the queries for the request.
        /// </summary>
        /// <param name="queries">The queries to append to the URL.</param>
        /// <returns>Request with queries added.</returns>
        public ITikTokHttpRequest SetQueries(IDictionary<string, object> queries)
        {
            if (queries == null) return this;
            this.query = string.Join("&", queries.Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value.ToString())}"));
            return this;
        }

        /// <summary>
        /// Sends the request and returns the response.
        /// </summary>
        /// <returns>The value in the response.</returns>
        /// <exception cref="HttpRequestException">If the request was unsuccessful</exception>
        private async Task<HttpContent> GetContent()
        {
            if (query != null)
                this.request.RequestUri = new Uri($"{this.request.RequestUri.AbsoluteUri}?{query}");
            var response = await client.SendAsync(this.request);
            this.request.Dispose();
            this.sent = true;
            if (!response.IsSuccessStatusCode) throw new HttpRequestException("Request was unsuccessful");
            var ct = response.Content.Headers?.ContentType;
            if (ct?.CharSet != null)
                ct.CharSet = ct.CharSet.Replace("\"", "");
            response.Headers.TryGetValues("Set-Cookie", out var vals);
            if (vals != null)
            {
                foreach (var val in vals)
                {
                    var cookie = val.Split(';')[0].Split('=');
                    cookieJar[cookie[0]] = cookie[1];
                }
            }
            return response.Content;
        }
    }
}