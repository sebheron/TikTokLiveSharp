using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Errors
{
    public class ChatMessageSendFailureException : Exception
    {
        public ChatMessageSendFailureException()
        {
        }

        public ChatMessageSendFailureException(string message) : base(message)
        {
        }

        public ChatMessageSendFailureException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
