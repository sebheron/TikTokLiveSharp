using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Errors
{
    public class InvalidSessionIdException : Exception
    {
        public InvalidSessionIdException()
        {
        }

        public InvalidSessionIdException(string message) : base(message)
        {
        }

        public InvalidSessionIdException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
