using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Events
{
    public class ConnectionEventArgs
    {
        public ConnectionEventArgs(bool isConnected)
        {
            this.IsConnected = isConnected;
        }
    
        public bool IsConnected { get; }
    }
}
