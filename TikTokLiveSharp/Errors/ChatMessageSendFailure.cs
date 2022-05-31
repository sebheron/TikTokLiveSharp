using System;

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