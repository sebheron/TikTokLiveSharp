using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace TikTokLiveSharp.Client.Proxy
{
    internal class ProxyContainer
    {
        private readonly string[] proxies;
        private int index;

        /// <summary>
        /// Builds a proxy container object.
        /// Uses defaults:
        /// Enabled = true,
        /// Mode = RotationSettings.CONSECUTIVE
        /// </summary>
        /// <param name="proxies">List of proxies</param>
        internal ProxyContainer(params string[] proxies) :
            this(RotationSettings.CONSECUTIVE, true, proxies) { }

        /// <summary>
        /// Builds a proxy container object.
        /// Uses defaults:
        /// Mode = RotationSettings.CONSECUTIVE
        /// </summary>
        /// <param name="enabled">Enabled</param>
        /// <param name="proxies">List of proxies</param>
        internal ProxyContainer(bool enabled, params string[] proxies)
            : this(RotationSettings.CONSECUTIVE, enabled, proxies) { }

        /// <summary>
        /// Builds a proxy container object.
        /// Uses defaults:
        /// Enabled = true
        /// </summary>
        /// <param name="mode">Rotation mode</param>
        /// <param name="proxies">List of proxies</param>
        internal ProxyContainer(RotationSettings mode, params string[] proxies)
            : this(mode, true, proxies) { }

        /// <summary>
        /// Builds a proxy container object.
        /// Uses no default values.
        /// </summary>
        /// <param name="mode">Rotation mode</param>
        /// <param name="enabled">Enabled</param>
        /// <param name="proxies">List of proxies</param>
        internal ProxyContainer(RotationSettings mode, bool enabled, params string[] proxies)
        {
            this.Mode = mode;
            this.Enabled = enabled;
            this.proxies = proxies;
            this.index = 0;
        }

        /// <summary>
        /// Gets the current proxy.
        /// </summary>
        /// <returns>Proxy string.</returns>
        public string Get()
        {
            int index;

            if (this.Count < 1 || !this.Enabled)
            {
                return null;
            }

            if (this.Mode == RotationSettings.CONSECUTIVE)
            {
                if (this.index >= this.Count)
                {
                    this.index = 0;
                }

                index = this.index;
                this.index++;
            }
            else
            {
                index = new Random().Next(this.Count - 1);
            }

            return proxies[index];
        } 

        /// <summary>
        /// Whether the proxy container is enabled (or not).
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The current mode of the proxy container.
        /// </summary>
        public RotationSettings Mode { get; private set; }

        /// <summary>
        /// Gets the number of proxies.
        /// </summary>
        public int Count => proxies.Length;
    }
}
