using System;

namespace TikTokLiveSharp.Errors
{
    public class SignatureRateLimitReachedException : Exception
    {
        public SignatureRateLimitReachedException()
        {
        }

        public SignatureRateLimitReachedException(string message) : base(message)
        {
        }

        public SignatureRateLimitReachedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
