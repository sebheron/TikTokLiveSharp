using System;

namespace TikTokLiveSharp.Errors
{
    public class FailedFetchGiftsException : Exception
    {
        public FailedFetchGiftsException()
        {
        }

        public FailedFetchGiftsException(string message) : base(message)
        {
        }

        public FailedFetchGiftsException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}