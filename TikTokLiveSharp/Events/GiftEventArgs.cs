using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Events
{
    public class GiftEventArgs
    {
        public GiftEventArgs(string userID, object gift)
        {
            this.UserID = userID;
            this.Gift = gift;
        }

        public string UserID { get; }
        public object Gift { get; }
    }
}
