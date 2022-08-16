using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TikTokLiveSharp.Client.Requests;

namespace TikTokLiveSharp.Client.Sockets
{
    public class TikTokWebSocket : ITikTokWebSocket
    {
        private ClientWebSocket clientWebSocket;

        /// <summary>
        /// Creates a TikTok websocket instance.
        /// </summary>
        /// <param name="cookieContainer">The cookie container to use.</param>
        public TikTokWebSocket(TikTokCookieJar cookieContainer)
        {
            this.clientWebSocket = new ClientWebSocket();

            this.clientWebSocket.Options.AddSubProtocol("echo-protocol");
            this.clientWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);

            var cookieHeader = new StringBuilder();
            foreach (var cookie in cookieContainer)
            {
                cookieHeader.Append(cookie);
            }
            this.clientWebSocket.Options.SetRequestHeader("Cookie", cookieHeader.ToString());
        }

        /// <summary>
        /// Connect to the websocket.
        /// </summary>
        /// <param name="url">Websocket url.</param>
        /// <returns>Task to await.</returns>
        public async Task Connect(string url)
        {
            await this.clientWebSocket.ConnectAsync(new Uri(url), CancellationToken.None);
        }

        /// <summary>
        /// Disconnects from the websocket.
        /// </summary>
        /// <returns>Task to await.</returns>
        public async Task Disconnect()
        {
            await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }

        /// <summary>
        /// Writes a message to the websocket.
        /// </summary>
        /// <param name="arr">The bytes to write.</param>
        /// <returns>Task to await.</returns>
        public async Task WriteMessage(ArraySegment<byte> arr)
        {
            await this.clientWebSocket.SendAsync(arr, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        /// <summary>
        /// Recieves a message from websocket.
        /// </summary>
        /// <returns></returns>
        public async Task<TikTokWebSocketResponse> RecieveMessage()
        {
            var arr = new ArraySegment<byte>(new byte[100000]);
            var response = await this.clientWebSocket.ReceiveAsync(arr, CancellationToken.None);
            if (response.MessageType == WebSocketMessageType.Binary)
            {
                return new TikTokWebSocketResponse(arr.Array, response.Count);
            }
            return null;
        }

        /// <summary>
        /// Is the websocket currently connected.
        /// </summary>
        public bool IsConnected => this.clientWebSocket.State == WebSocketState.Open;
    }
}
