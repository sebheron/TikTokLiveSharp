using System;
using System.Collections;
using System.Collections.Generic;

namespace TikTokLiveSharp.Client.Requests
{
    public class TikTokCookieJar : IEnumerable<string>
    {
        private IDictionary<string, string> cookies;

        public TikTokCookieJar()
        {
            this.cookies = new Dictionary<string, string>();
        }

        public string this[string index]
        {
            get
            {
                return this.cookies[index];
            }
            set
            {
                this.cookies[index] = value;
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
