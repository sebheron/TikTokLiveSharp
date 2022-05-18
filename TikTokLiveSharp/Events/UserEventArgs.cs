using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Events
{
    public class UserEventArgs
    {
        public UserEventArgs(string userID)
        {
            this.UserID = userID;
        }

        public string UserID { get; }
    }
}
