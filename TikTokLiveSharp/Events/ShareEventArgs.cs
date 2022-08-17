using TikTokLiveSharp.Models;

namespace TikTokLiveSharp.Events
{
    public class ShareEventArgs
    {
        public ShareEventArgs(User user, int count)
        {
            this.User = user;
            this.Count = count;
        }

        public User User { get; }

        public int Count { get; }
    }
}
