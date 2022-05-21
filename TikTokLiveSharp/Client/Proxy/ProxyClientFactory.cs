using Flurl.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Linq;

namespace TikTokLiveSharp.Client.Proxy
{
    public class ProxyClientFactory : DefaultHttpClientFactory
    {
        /// <summary>
        /// Creates an instance of proxy client factory.
        /// </summary>
        /// <param name="isEnabled">Are proxies enabled.</param>
        /// <param name="settings">The inital rotation settings to use.</param>
        /// <param name="addresses">The list of inital addresses.</param>
        public ProxyClientFactory(bool isEnabled = false,
            RotationSettings settings = RotationSettings.CONSECUTIVE,
            params string[] addresses)
        {
            this.IsEnabled = isEnabled;
            this.Settings = settings;
            this.Addresses = addresses.ToList();
        }

        public override HttpMessageHandler CreateMessageHandler()
        {
            return new HttpClientHandler
            {
                Proxy = this.GetProxy(),
                UseProxy = this.IsEnabled
            };
        }

        /// <summary>
        /// Gets the current proxy to use.
        /// </summary>
        /// <returns>WebProxy for the current indexed address.</returns>
        private WebProxy GetProxy()
        {
            if (!IsEnabled) return null;
            var address = this.Addresses.FirstOrDefault();
            if (this.Addresses.Count <= 0) return null;
            switch (Settings)
            {
                case RotationSettings.CONSECUTIVE:
                    address = this.Addresses[this.Index];
                    this.Index = (this.Index + 1) % this.Count;
                    break;
                case RotationSettings.RANDOM:
                    this.Index = new Random().Next(this.Count - 1);
                    address = this.Addresses[this.Index];
                    break;
                case RotationSettings.PINNED:
                    address = this.Addresses[this.Index];
                    break;
            }
            return new WebProxy(address);
        }

        /// <summary>
        /// The rotation settings.
        /// </summary>
        public RotationSettings Settings { get; set; }

        /// <summary>
        /// Whether the proxies should be enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// List of addresses
        /// </summary>
        public List<string> Addresses;

        private int index;
        /// <summary>
        /// The index of the current address.
        /// </summary>
        public int Index
        {
            get => this.index;
            set
            {
                if (this.index < 0 || this.index > this.Count - 1)
                    throw new IndexOutOfRangeException();
                this.index = value;
            }
        }

        /// <summary>
        /// The number of currently available addresses.
        /// </summary>
        public int Count => this.Addresses.Count;
    }
}
