using System;

namespace TikTokLiveSharp.Errors
{
    public class FailedConnectionException : Exception
    {
        public FailedConnectionException()
        {
        }

        public FailedConnectionException(string message) : base(message)
        {
        }

        public FailedConnectionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}