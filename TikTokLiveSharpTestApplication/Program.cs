using System;
using TikTokLiveSharp.Client;
using TikTokLiveSharp.Protobuf;

namespace TikTokLiveSharpTestApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter a username:");
            var client = new TikTokLiveClient(Console.ReadLine());
            client.OnCommentRecieved += Client_OnCommentRecieved;
            client.Run();
        }

        private static void Client_OnCommentRecieved(object sender, WebcastChatMessage e)
        {
            Console.WriteLine($"{e.User.uniqueId}: {e.Comment}");
        }
    }
}
