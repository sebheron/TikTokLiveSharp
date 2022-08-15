using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using TikTokLiveSharp.Client.Requests;
using System.Linq;

namespace TikTokLiveSharp.Client.Sockets
{
    public class TikTokWebSocket
    {
        private ClientWebSocket clientWebSocket;

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

        public async Task Connect(string url)
        {
            await this.clientWebSocket.ConnectAsync(new Uri(url), CancellationToken.None);
        }

        public async Task WriteMessage(ArraySegment<byte> arr)
        {
            await this.clientWebSocket.SendAsync(arr, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        public async Task<byte[]> RecieveMessage()
        {
            var arr = new ArraySegment<byte>(new byte[100000]);
            var response = await this.clientWebSocket.ReceiveAsync(arr, CancellationToken.None);
            arr.Array.ToList().RemoveAll(x => x == 0);
            if (response.MessageType == WebSocketMessageType.Binary)
            {
                return arr.Array.Take(response.Count).ToArray();
            }
            return null;
        }

        public bool IsConnected => this.clientWebSocket.State == WebSocketState.Open;
    }
}
