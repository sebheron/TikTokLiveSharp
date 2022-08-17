using Newtonsoft.Json.Linq;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TikTokLiveSharp.Client.Proxy;
using TikTokLiveSharp.Client.Requests;
using TikTokLiveSharp.Client.Sockets;
using TikTokLiveSharp.Errors;
using TikTokLiveSharp.Models;

namespace TikTokLiveSharp.Client
{
    public abstract class TikTokBaseClient
    {
        protected Dictionary<string, object> clientParams;
        protected TikTokHTTPClient http;
        protected TikTokWebSocket socket;
        protected bool connecting, isPolling, processInitialData, fetchRoomInfoOnConnect, enableExtendedGiftInfo;
        protected TimeSpan pollingInterval;
        private Dictionary<int, TikTokGift> availableGifts;
        private string roomID;
        private JObject roomInfo;
        private Task runningTask, pollingTask;
        private CancellationToken token;
        private string uniqueID;
        private int? viewerCount;

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
            this.availableGifts = new Dictionary<int, TikTokGift>();
            this.roomID = null;
            this.viewerCount = null;
            this.connecting = false;

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
            this.pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(3);
            this.processInitialData = processInitialData;
            this.fetchRoomInfoOnConnect = fetchRoomInfoOnConnect;
            this.enableExtendedGiftInfo = enableExtendedGiftInfo;
        }

        public Dictionary<int, TikTokGift> AvailableGifts => this.availableGifts;

        public bool Connected => this.socket?.IsConnected ?? false;

        public string RoomID => this.roomID;

        public JObject RoomInfo => this.roomInfo;

        public string UniqueID => this.uniqueID;

        public int? ViewerCount => this.viewerCount;

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
            this.pollingTask.Wait();
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
        /// Stops the client.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        public async Task Stop()
        {
            if (this.Connected)
            {
                await this.Disconnect();
            }
        }

        /// <summary>
        /// Connect.
        /// </summary>
        /// <returns>The room id.</returns>
        /// <exception cref="AlreadyConnectingException">In the process of connecting.</exception>
        /// <exception cref="AlreadyConnectedException">Already connected.</exception>
        /// <exception cref="FailedConnectionException">Failed to connect.</exception>
        protected virtual async Task<string> Connect()
        {
            if (this.connecting) throw new AlreadyConnectingException();
            if (this.Connected) throw new AlreadyConnectedException();

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

                await this.FetchRoomData();
                token.ThrowIfCancellationRequested();

                return this.roomID;
            }
            catch (Exception e)
            {
                throw new FailedConnectionException(e.Message);
            }
        }

        /// <summary>
        /// Disconnect.
        /// </summary>
        /// <returns>Task to await.</returns>
        protected virtual async Task Disconnect()
        {
            this.isPolling = false;
            this.roomInfo = null;
            this.connecting = false;
            if (this.Connected)
            {
                await this.socket.Disconnect();
            }
            this.clientParams["cursor"] = "";
            await this.runningTask;
            await this.pollingTask;
        }

        /// <summary>
        /// Fetch the currently available giftTokens.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected async Task<Dictionary<int, TikTokGift>> FetchAvailableGifts()
        {
            try
            {
                var response = await this.http.GetJObjectFromWebcastAPI("gift/list/", this.clientParams);
                var giftTokens = response.SelectTokens("..gifts")?.FirstOrDefault()?.Children() ?? null;
                if (giftTokens == null) return new Dictionary<int, TikTokGift>();
                foreach (var giftToken in giftTokens)
                {
                    var gift = giftToken.ToObject<TikTokGift>();
                    this.availableGifts[giftToken.SelectToken(".id").Value<int>()] = gift;
                }
                return this.availableGifts;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, new FailedFetchGiftsException("Failed to fetch giftTokens from WebCast, see stacktrace for more info."));
            }
        }

        /// <summary>
        /// Fetches the room data.
        /// </summary>
        /// <returns>Task to await.</returns>
        protected async Task FetchRoomData()
        {
            var webcastResponse = await this.http.GetDeserializedMessage("im/fetch/", this.clientParams, true);

            this.clientParams["cursor"] = webcastResponse.Cursor;
            this.clientParams["internal_ext"] = webcastResponse.internalExt;

            try
            {
                if (webcastResponse.wsUrl != null && webcastResponse.wsParam != null)
                {
                    await this.BeginWebsocket(webcastResponse);
                }
            }
            catch (Exception e)
            {
                throw new FailedConnectionException("Failed to connect to the websocket", e);
            }


            this.HandleWebcastMessages(webcastResponse);
        }

        /// <summary>
        /// Begins the websocket connection.
        /// </summary>
        /// <param name="webcastResponse">The first webcast response with websocket details.</param>
        /// <returns>Task to await.</returns>
        protected async Task BeginWebsocket(WebcastResponse webcastResponse)
        {
            this.clientParams[webcastResponse.wsParam.Name] = webcastResponse.wsParam.Value;
            var url = webcastResponse.wsUrl + "?" + string.Join("&", this.clientParams.Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value.ToString())}"));
            this.socket = new TikTokWebSocket(TikTokHttpRequest.CookieJar);
            await this.socket.Connect(url);
            this.runningTask = Task.Run(this.WebSocketLoop, token);
            this.pollingTask = Task.Run(this.PingLoop, token);
        }

        /// <summary>
        /// Loops a connection to the websocket.
        /// </summary>
        /// <returns>Task to await.</returns>
        protected async Task WebSocketLoop()
        {
            while (true)
            {
                var response = await this.socket.RecieveMessage();
                if (response == null) continue;
                try
                {
                    using (var websocketMessageStream = new MemoryStream(response.Array, 0, response.Count))
                    {
                        var websocketMessage = Serializer.Deserialize<WebcastWebsocketMessage>(websocketMessageStream);

                        if (websocketMessage.Binary != null)
                        {
                            using (var messageStream = new MemoryStream(websocketMessage.Binary))
                            {
                                var message = Serializer.Deserialize<WebcastResponse>(messageStream);

                                await this.SendAcknowledgement(websocketMessage.Id);

                                this.HandleWebcastMessages(message);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new WebSocketException("Websocket has likely been closed.", e);
                }
            }
        }

        /// <summary>
        /// Pings the websocket.
        /// </summary>
        /// <returns>Task to await.</returns>
        protected async Task PingLoop()
        {
            while (true)
            {
                using (var messageStream = new MemoryStream())
                {
                    await this.socket.WriteMessage(new ArraySegment<byte>(new byte[] { 58, 2, 104, 98 }));
                }
                await Task.Delay(10);
            }
        }

        /// <summary>
        /// Send an acknowlegement to the websocket.
        /// </summary>
        /// <param name="id">The acknowledgment id.</param>
        /// <returns>Task to await.</returns>
        protected async Task SendAcknowledgement(ulong id)
        {
            using (var messageStream = new MemoryStream())
            {
                Serializer.Serialize<WebcastWebsocketAck>(messageStream, new WebcastWebsocketAck
                {
                    Type = "ack",
                    Id = id
                });
                await this.socket.WriteMessage(new ArraySegment<byte>(messageStream.ToArray()));
            }
        }

        /// <summary>
        /// Fetch the current room id.
        /// </summary>
        /// <returns>The room id.</returns>
        /// <exception cref="Exception">Failed to fetch room id.</exception>
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

        /// <summary>
        /// Fetch the current room info.
        /// </summary>
        /// <returns>The room information as a JObject.</returns>
        /// <exception cref="Exception">Failed to fetch room information.</exception>
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

        /// <summary>
        /// Handles the webcast messages
        /// </summary>
        /// <param name="webcastResponse">The current webcast response.</param>
        protected abstract void HandleWebcastMessages(WebcastResponse webcastResponse);
    }
}