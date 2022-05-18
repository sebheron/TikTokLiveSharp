using System;
using System.Collections.Generic;
using System.Text;

namespace TikTokLiveSharp.Events
{
    public class ViewerCountEventArgs
    {
        public ViewerCountEventArgs(int viewerCount)
        {
            this.ViewerCount = viewerCount;
        }

        public int ViewerCount { get; }
    }
}
