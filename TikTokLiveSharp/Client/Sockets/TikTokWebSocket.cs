using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace TikTokLiveSharp.Client.Sockets
{
    public class TikTokWebSocket
    {
        private ClientWebSocket clientWebSocket;

        public TikTokWebSocket(string url, CookieContainer cookieContainer)
        {
            this.clientWebSocket = new ClientWebSocket();
            this.clientWebSocket.Options.AddSubProtocol("echo-protocol");
            this.clientWebSocket.Options.Cookies = cookieContainer;
            this.clientWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        }

        public async Task Connect(string url, CancellationToken token)
        {
            await this.clientWebSocket.ConnectAsync(new Uri(url), token);
        }

        public bool IsConnected => this.clientWebSocket.State == WebSocketState.Open;
    }
}
