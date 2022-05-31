using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace TikTokLiveSharp.Client.Proxy
{
    public class RotatingProxy : IWebProxy
    {
        /// <summary>
        /// List of addresses
        /// </summary>
        public List<string> Addresses;

        private int index;

        /// <summary>
        /// Creates an instance of proxy client factory.
        /// </summary>
        /// <param name="isEnabled">Are proxies enabled.</param>
        /// <param name="settings">The inital rotation settings to use.</param>
        /// <param name="addresses">The list of inital addresses.</param>
        public RotatingProxy(bool isEnabled = false,
            RotationSettings settings = RotationSettings.CONSECUTIVE,
            params string[] addresses)
        {
            this.IsEnabled = isEnabled;
            this.Settings = settings;
            this.Addresses = addresses.ToList();
        }

        /// <summary>
        /// The number of currently available addresses.
        /// </summary>
        public int Count => this.Addresses.Count;

        /// <summary>
        /// Not implemented
        /// </summary>
        public ICredentials Credentials { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
        /// Whether the proxies should be enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The rotation settings.
        /// </summary>
        public RotationSettings Settings { get; set; }

        public Uri GetProxy(Uri destination)
        {
            if (!IsEnabled) return destination;
            if (this.Addresses.Count <= 0) return destination;
            switch (Settings)
            {
                case RotationSettings.CONSECUTIVE:
                    var address = this.Addresses[this.Index];
                    this.Index = (this.Index + 1) % this.Count;
                    return new Uri(address);

                case RotationSettings.RANDOM:
                    this.Index = new Random().Next(this.Count - 1);
                    return new Uri(this.Addresses[this.Index]);

                case RotationSettings.PINNED:
                    return new Uri(this.Addresses[this.Index]);

                default:
                    return destination;
            }
        }

        public bool IsBypassed(Uri host)
        {
            return IsEnabled;
        }
    }
}