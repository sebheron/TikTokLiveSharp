using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Errors
{
    public class FailedFetchRoomInfoException : Exception
    {
        public FailedFetchRoomInfoException()
        {
        }

        public FailedFetchRoomInfoException(string message) : base(message)
        {
        }

        public FailedFetchRoomInfoException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
