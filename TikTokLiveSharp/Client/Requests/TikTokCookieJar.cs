using System;
using System.Collections;
using System.Collections.Generic;

namespace TikTokLiveSharp.Client.Requests
{
    public class TikTokCookieJar : IEnumerable<string>
    {
        private IDictionary<string, string> cookies;

        /// <summary>
        /// Create a TikTok cookie jar instance.
        /// </summary>
        public TikTokCookieJar()
        {
            this.cookies = new Dictionary<string, string>();
        }

        /// <summary>
        /// Get the cookie by key.
        /// </summary>
        /// <param name="key">The cookie key.</param>
        /// <returns>Cookie value.</returns>
        public string this[string key]
        {
            get
            {
                return this.cookies[key];
            }
            set
            {
                this.cookies[key] = value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var cookie in cookies)
            {
                yield return $"{cookie.Key}={cookie.Value};";
            }
        }
    }
}
