using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Events
{
    public class QuestionEventArgs
    {
        public QuestionEventArgs(string userID, string question)
        {

        }

        public string UserID { get; }
        public string Question { get; }
    }
}
