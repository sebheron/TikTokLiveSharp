using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Events
{
    public class CommentEventArgs
    {
        public CommentEventArgs(string userID, string comment)
        {
            this.UserID = userID;
            this.Comment = comment;
        }

        public string UserID { get; }
        public string Comment { get; }
    }
}
