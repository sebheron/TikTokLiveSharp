using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TikTokLiveSharp.Client.Proxy;
using TikTokLiveSharp.Errors;
using TikTokLiveSharp.Protobuf;

namespace TikTokLiveSharp.Client
{
    public abstract class TikTokBaseClient
    {
        private string uniqueID;
        private JObject roomInfo;
        private Dictionary<int, JToken> availableGifts;
        private string roomID;
        private int? viewerCount;
        private bool connecting;
        private bool connected;
        private Task runningTask;
        private CancellationToken token;

        protected Dictionary<string, object> clientParams;
        protected TikTokHTTPClient http;
        protected TimeSpan pollingInterval;
        protected bool isPolling, processInitialData, fetchRoomInfoOnConnect, enableExtendedGiftInfo;

        public TikTokBaseClient(string uniqueID,
            TimeSpan? timeout = null,
            TimeSpan? pollingInterval = null,
            Dictionary<string, object> clientParams = null,
            bool processInitialData = true,
            bool fetchRoomInfoOnConnect = true,
            bool enableExtendedGiftInfo = true,
            RotatingProxy proxyHandler = null,
            string lang = "en-US")
        {
            this.uniqueID = uniqueID;
            this.roomInfo = null;
            this.availableGifts = new Dictionary<int, JToken>();
            this.roomID = null;
            this.viewerCount = null;
            this.connecting = false;
            this.connected = false;

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

            this.http = new TikTokHTTPClient(timeout, proxyHandler);
            this.pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(1);
            this.processInitialData = processInitialData;
            this.fetchRoomInfoOnConnect = fetchRoomInfoOnConnect;
            this.enableExtendedGiftInfo = enableExtendedGiftInfo;
        }

        protected async Task<string> FetchRoomId()
        {
            try
            {
                var html = await this.http.GetLivestreamPage(this.uniqueID);
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

        protected async Task<JObject> FetchRoomInfo()
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

        protected async Task<Dictionary<int, JToken>> FetchAvailableGifts()
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

        protected async Task FetchRoomPolling()
        {
            this.isPolling = true;

            while (this.isPolling)
            {
                try
                {
                    await this.FetchRoomData();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message, new FailedRoomPollingException("Failed to retrieve events from WebCast, see stacktrace for more info."));
                }

                await Task.Delay(this.pollingInterval);
                token.ThrowIfCancellationRequested();
            }
        }

        protected async Task FetchRoomData(bool isInitial = false)
        {
            var webcastResponse = await this.http.GetDeserializedMessage("im/fetch/", this.clientParams);

            if (webcastResponse.Cursor != "0")
            {
                this.clientParams["cursor"] = webcastResponse.Cursor;
            }

            if (isInitial && !this.processInitialData) return;

            this.HandleWebcastMessages(webcastResponse);
        }

        protected abstract void HandleWebcastMessages(WebcastResponse webcastResponse);

        protected virtual async Task<string> Connect()
        {
            if (this.connecting) throw new AlreadyConnectingException();
            if (this.connected) throw new AlreadyConnectedException();

            this.connecting = true;

            try
            {
                await this.FetchRoomId();
                token.ThrowIfCancellationRequested();

                if (this.fetchRoomInfoOnConnect)
                {
                    var status = (await this.FetchRoomInfo()).SelectToken(".data").SelectToken(".status");
                    if (status == null || status.Value<int>() == 4)
                    {
                        throw new LiveNotFoundException();
                    }
                }
                token.ThrowIfCancellationRequested();

                if (this.enableExtendedGiftInfo)
                {
                    await this.FetchAvailableGifts();
                }
                token.ThrowIfCancellationRequested();

                await this.FetchRoomData(true);
                token.ThrowIfCancellationRequested();
                this.connected = true;

                this.runningTask = Task.Run(this.FetchRoomPolling, token);

                return this.roomID;
            }
            catch (Exception e)
            {
                throw new FailedConnectionException(e.Message);
            }
        }

        protected virtual async Task Disconnect()
        {
            this.isPolling = false;
            this.roomInfo = null;
            this.connecting = false;
            this.connected = false;
            this.clientParams["cursor"] = "";
            await this.runningTask;
        }

        protected async Task<JObject> RetrieveRoomInfo()
        {
            if (!this.connected)
            {
                await this.FetchRoomId();
            }
            return await this.FetchRoomInfo();
        }

        /// <summary>
        /// Stops the client.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        public async Task Stop()
        {
            if (this.connected)
            {
                await this.Disconnect();
            }
        }

        /// <summary>
        /// Async method to start the client.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <param name="retryConnection">Recurssively attempt to start a connection until either a connection is formed or cancellation is requested.</param>
        /// <returns>The room ID.</returns>
        public async Task<string> Start(CancellationToken? cancellationToken = null, bool retryConnection = false)
        {
            this.token = cancellationToken ?? new CancellationToken();
            token.ThrowIfCancellationRequested();
            try
            {
                return await this.Connect();
            }
            catch (FailedConnectionException)
            {
                if (retryConnection)
                {
                    await Task.Delay(this.pollingInterval);
                    return await Start(cancellationToken, retryConnection);
                }
                return null;
            }
        }

        /// <summary>
        /// Async method to start the client.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        public void Run(CancellationToken? cancellationToken = null, bool retryConnection = false)
        {
            this.token = cancellationToken ?? new CancellationToken();
            token.ThrowIfCancellationRequested();
            var run = Task.Run(() => this.Start(token, retryConnection), token);
            run.Wait();
            this.runningTask.Wait();
        }

        public int? ViewerCount => this.viewerCount;

        public string RoomID => this.roomID;

        public JObject RoomInfo => this.roomInfo;

        public string UniqueID => this.uniqueID;

        public bool Connected => this.connected;

        public Dictionary<int, JToken> AvailableGifts => this.availableGifts;
    }
}
