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

        public TikTokWebSocket(string url, TikTokCookieJar cookieContainer)
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
            await this.clientWebSocket.ConnectAsync(new Uri(url), new CancellationTokenSource(15000).Token);
        }

        public async Task WriteMessage(ArraySegment<byte> arr)
        {
            await this.clientWebSocket.SendAsync(arr, WebSocketMessageType.Binary, false, new CancellationTokenSource(15000).Token);
        }

        public async Task<byte[]> RecieveMessage()
        {
            var arr = new ArraySegment<byte>(new byte[8192]);
            var response = await this.clientWebSocket.ReceiveAsync(arr, new CancellationTokenSource(15000).Token);
            var i = 0;
            for (int j = 0; j < 8192; j++)
            {
                if (arr.Array[j] != 0)
                {
                    i = j;
                }
            }
            return arr.Array.Take(i).ToArray();
        }

        public bool IsConnected => this.clientWebSocket.State == WebSocketState.Open;
    }
}
