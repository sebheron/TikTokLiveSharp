using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Errors
{
    public class ChatMessageRepeatException : Exception
    {
        public ChatMessageRepeatException()
        {
        }

        public ChatMessageRepeatException(string message) : base(message)
        {
        }

        public ChatMessageRepeatException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
