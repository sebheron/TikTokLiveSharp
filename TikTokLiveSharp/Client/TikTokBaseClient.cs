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
        private string userID;
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
            ProxyClientFactory proxyClientFactory = null,
            string lang = "en-US")
        {
            this.userID = userID;
            this.roomInfo = null;
            this.availableGifts = new Dictionary<int, JToken>();
            this.roomID = null;
            this.viewerCount = null;
            this.connecting = false;
            this.connected = false;
            this.sessionID = null;

            this.clientParams = new Dictionary<string, object>();
            foreach (var parameter in TikTokRequestSettings.DEFAULT_CLIENT_PARAMS)
            {
                this.clientParams[parameter.Key] = parameter.Value;
            }
            for (int i = 0; i < (clientParams?.Count ?? 0); i++)
            {
                var vals = clientParams.ElementAt(i);
                this.clientParams[vals.Key] = vals.Value;
            }

            this.clientParams["app_language"] = lang;
            this.clientParams["webcast_language"] = lang;

            this.http = new TikTokHTTPClient(timeout, proxyClientFactory);
            this.pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(1);
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
                    this.roomID = id;
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
                var response = await this.http.GetJObjectFromWebcastAPI("gift/list/", this.clientParams);
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

            if (webcastResponse.Cursor != "0")
            {
                this.clientParams["cursor"] = webcastResponse.Cursor;
            }

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
                    var status = (await this.fetchRoomInfo()).SelectToken(".data").SelectToken(".status");
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

        protected async Task<JObject> retrieveRoomInfo()
        {
            if (!this.connected)
            {
                await this.fetchRoomId();
            }
            return await this.fetchRoomInfo();
        }

        public async Task Stop()
        {
            if (this.connected)
            {
                await this.disconnect();
            }
        }

        public async Task<string> Start(string sessionID = null)
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

        public int? ViewerCount => this.viewerCount;

        public string RoomID => this.roomID;

        public JObject RoomInfo => this.roomInfo;

        public string UserID => this.userID;

        public bool Connected => this.connected;

        public Dictionary<int, JToken> AvailableGifts => this.availableGifts;
    }
}
