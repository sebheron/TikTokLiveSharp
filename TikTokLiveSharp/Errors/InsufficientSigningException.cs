using System;

namespace TikTokLiveSharp.Errors
{
    public class InsuffcientSigningException : Exception
    {
        public InsuffcientSigningException()
        {
        }

        public InsuffcientSigningException(string message) : base(message)
        {
        }

        public InsuffcientSigningException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}