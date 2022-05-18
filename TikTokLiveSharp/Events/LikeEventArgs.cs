using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Events
{
    public class LikeEventArgs
    {
        public LikeEventArgs(string userID, int likesCount, int totalLikeCount)
        {
            this.UserID = userID;
            this.LikesCount = likesCount;
            this.TotalLikeCount = totalLikeCount;
        }

        public string UserID { get; }
        public int LikesCount { get; }
        public int TotalLikeCount { get; }
    }
}
