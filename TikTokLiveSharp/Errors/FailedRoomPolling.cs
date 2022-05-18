using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Errors
{
    public class FailedRoomPollingException : Exception
    {
        public FailedRoomPollingException()
        {
        }

        public FailedRoomPollingException(string message) : base(message)
        {
        }

        public FailedRoomPollingException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
