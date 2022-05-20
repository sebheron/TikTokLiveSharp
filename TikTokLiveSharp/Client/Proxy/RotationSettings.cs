using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Client.Proxy
{
    /// <summary>
    /// Rotation settings for a proxy container.
    /// </summary>
    public enum RotationSettings
    {
        /// <summary>
        /// Rotate proxies consecutively, from proxy 0 -> 1 -> 2 -> ...etc.
        /// </summary>
        CONSECUTIVE = 1,
        /// <summary>
        /// Rotate proxies randomly, from proxy 0 -> 69 -> 420 -> 1 -> ...etc.
        /// </summary>
        RANDOM = 2,
        /// <summary>
        /// Don't rotate proxies at all, pin to a specific proxy index with SetPinned()
        /// </summary>
        PINNED = 3
    }
}
