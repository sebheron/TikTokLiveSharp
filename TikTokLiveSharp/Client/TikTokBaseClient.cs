using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TikTokLiveSharp.Client.Proxy;
using TikTokLiveSharp.Errors;
using TikTokLiveSharp.Protobuf;

namespace TikTokLiveSharp.Client
{
    public abstract class TikTokBaseClient
    {
        private static readonly Dictionary<string, object> DEFAULT_CLIENT_PARAMS = new Dictionary<string, object>()
        {
            { "aid", 1988 },
            { "app_name", "tiktok_web" },
            { "browser_name", "Mozilla" },
            { "browser_online", true },
            { "browser_platform", "Win32" },
            { "version_code", 180800 },
            { "browser_version", "5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36" },
            { "cookie_enabled", true },
            { "cursor", "" },
            { "device_platform", "web" },
            { "did_rule", 3 },
            { "fetch_rule", 1 },
            { "identity", "audience" },
            { "internal_ext", "" },
            { "last_rtt", 0 },
            { "live_id", 12 },
            { "resp_content_type", "protobuf" },
            { "screen_height", 1152 },
            { "screen_width", 2048 },
            { "tz_name", "Europe/Berlin" },
            { "browser_language", "en" },
            { "priority_region", "US" },
            { "region", "US" }
        };

        private string userID;
        private bool? discardExtraEvents;
        private JObject roomInfo;
        private Dictionary<int, JToken> availableGifts;
        private string roomID;
        private int? viewerCount;
        private bool connecting;
        private bool connected;
        private string sessionID;
        private Task runningTask;

        protected Dictionary<string, object> clientParams;
        protected TikTokHTTPClient http;
        protected TimeSpan pollingInterval;
        protected bool isPolling, processInitialData, fetchRoomInfoOnConnect, enableExtendedGiftInfo;

        public TikTokBaseClient(string userID,
            TimeSpan? timeout = null,
            TimeSpan? pollingInterval = null,
            Dictionary<string, object> clientParams = null,
            Dictionary<string, string> headers = null,
            bool processInitialData = true,
            bool fetchRoomInfoOnConnect = true,
            bool enableExtendedGiftInfo = true,
            ProxyContainer proxyContainer = null,
            string lang = "en-US")
        {
            this.userID = userID;
            this.discardExtraEvents = null;
            this.roomInfo = null;
            this.availableGifts = new Dictionary<int, JToken>();
            this.roomID = null;
            this.viewerCount = null;
            this.connecting = false;
            this.connected = false;
            this.sessionID = null;

            DEFAULT_CLIENT_PARAMS["app_language"] = lang;
            DEFAULT_CLIENT_PARAMS["webcast_language"] = lang;

            this.clientParams = new Dictionary<string, object>();
            foreach (var parameter in DEFAULT_CLIENT_PARAMS)
            {
                this.clientParams[parameter.Key] = parameter.Value;
            }
            for (int i = 0; i < (clientParams?.Count ?? 0); i++)
            {
                var vals = clientParams.ElementAt(i);
                this.clientParams[vals.Key] = vals.Value;
            }

            this.http = new TikTokHTTPClient(timeout, proxyContainer, headers);
            this.pollingInterval = pollingInterval ?? TimeSpan.FromMilliseconds(1000);
            this.processInitialData = processInitialData;
            this.fetchRoomInfoOnConnect = fetchRoomInfoOnConnect;
            this.enableExtendedGiftInfo = enableExtendedGiftInfo;
        }

        protected async Task<string> fetchRoomId()
        {
            try
            {
                var html = await this.http.GetLivestreamPage(this.userID);
                var first = Regex.Match(html, "room_id=([0-9]*)");
                var second = Regex.Match(html, "\"roomId\":\"([0 - 9] *)\"");
                var id = first.Groups[1]?.Value ?? (second.Groups[1]?.Value ?? String.Empty);
                if (!String.IsNullOrEmpty(id))
                {
                    this.clientParams["room_id"] = id;
                    return id;
                }
                throw new Exception(html.Contains("\"og:url\"") ? "User might be offline" : "Your IP or country might be blocked by TikTok.");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, new FailedFetchRoomInfoException("Failed to fetch room id from WebCast, see stacktrace for more info."));
            }
        }

        protected async Task<JObject> fetchRoomInfo()
        {
            try
            {
                var response = await this.http.GetJObjectFromWebcastAPI("room/info/", this.clientParams);
                this.roomInfo = response;
                return response;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, new FailedFetchRoomInfoException("Failed to fetch room info from WebCast, see stacktrace for more info."));
            }
        }

        protected async Task<Dictionary<int, JToken>> fetchAvailableGifts()
        {
            try
            {
                var response = await this.http.GetJObjectFromWebcastAPI(roomID, this.clientParams);
                var gifts = response.SelectToken("..gifts")?.Children();
                foreach (var gift in gifts)
                {
                    this.availableGifts[gift.SelectToken(".id").Value<int>()] = gift;
                }
                return this.availableGifts;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, new FailedFetchGiftsException("Failed to fetch gifts from WebCast, see stacktrace for more info."));
            }
        }

        protected async Task fetchRoomPolling()
        {
            this.isPolling = true;

            while (this.isPolling)
            {
                try
                {
                    await this.fetchRoomData();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message, new FailedRoomPollingException("Failed to retrieve events from WebCast, see stacktrace for more info."));
                }

                await Task.Delay(this.pollingInterval);
            }
        }

        protected async Task fetchRoomData(bool isInitial = false)
        {
            var webcastResponse = await this.http.GetDeserializedMessage("im/fetch/", this.clientParams);

            var lastCursor = this.clientParams["cursor"];
            var nextCursor = webcastResponse.Cursor;

            if (isInitial && !this.processInitialData) return;

            this.handleWebcastMessages(webcastResponse);
        }

        protected abstract void handleWebcastMessages(WebcastResponse webcastResponse);

        protected virtual async Task<string> connect()
        {
            if (this.connecting) throw new AlreadyConnectingException();
            if (this.connected) throw new AlreadyConnectedException();

            this.connecting = true;

            try
            {
                await this.fetchRoomId();

                if (this.fetchRoomInfoOnConnect)
                {
                    var status = (await this.fetchRoomInfo()).SelectToken(".status");
                    if (status == null || status.Value<int>() == 4)
                    {
                        throw new LiveNotFoundException();
                    }
                }

                if (this.enableExtendedGiftInfo)
                {
                    await this.fetchAvailableGifts();
                }

                await this.fetchRoomData(true);
                this.connected = true;

                this.runningTask = Task.Run(this.fetchRoomPolling);

                return this.roomID;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, new FailedConnectionException());
            }
        }

        protected virtual async Task disconnect()
        {
            this.isPolling = false;
            this.roomInfo = null;
            this.connecting = false;
            this.connected = false;
            this.clientParams["cursor"] = "";
            await this.runningTask;
        }

        public async Task stop()
        {
            if (this.connected)
            {
                await this.disconnect();
            }
        }

        public async Task<string> start(string sessionID = null)
        {
            this.sessionID = sessionID;
            return await this.connect();
        }

        public void Run(string sessionID = null)
        {
            this.sessionID = sessionID;
            var run = Task.Run(this.connect);
            run.Wait();
            this.runningTask.Wait();
        }

        public async Task<JObject> RetrieveRoomInfo()
        {
            if (!this.connected)
            {
                await this.fetchRoomId();
            }
            return await this.fetchRoomInfo();
        }

        public void SetProxiesEnabled(bool enabled)
        {
            this.http.proxyContainer.Enabled = enabled;
        }

        public void AddProxies(params string[] proxies)
        {
            this.http.proxyContainer.proxies.AddRange(proxies);
        }

        public void RemoveProxies(params string[] proxies)
        {
            this.http.proxyContainer.proxies.RemoveAll(x => proxies.Contains(x));
        }
        public IList<string> GetProxies()
        {
            return this.http.proxyContainer.proxies;
        }

        public int? ViewerCount => this.viewerCount;

        public string RoomID => this.roomID;

        public JObject RoomInfo => this.roomInfo;

        public string UserID => this.userID;

        public bool Connected => this.connected;

        public Dictionary<int, JToken> AvailableGifts => this.availableGifts;
    }
}
