# TikTokLiveSharp
![HitCount](https://hits.dwyl.com/sebheron/TikTokLiveSharp.svg?style=flat)
![Issues](https://img.shields.io/github/issues/sebheron/TikTokLiveSharp)
![Forks](https://img.shields.io/github/forks/sebheron/TikTokLiveSharp)
![Stars](https://img.shields.io/github/stars/sebheron/TikTokLiveSharp)
![Tweet](https://img.shields.io/twitter/url?url=https%3A%2F%2Fgithub.com%2Fsebheron%2FTikTokLiveSharp)

#### Read TikTok Live chat messages, gifts, etc.

A C# port of TikTok Live connector library (See [here](https://github.com/zerodytrash/TikTok-Live-Connector), [here](https://github.com/isaackogan/TikTokLive) and [here](https://github.com/Davincible/gotiktoklive) for more in-depth documentation).
The primary incentive behind designing this library was to allow direct implementation of the TikTok Live connector into Unity, it's implemented in .NET Standard and should work universally along all .NET supported platforms. An older version of Protobuf-net was used to ensure Unity compatibility.
````c#
var client = new TikTokLiveClient(uniqueID);
client.OnCommentRecieved += Client_OnCommentRecieved;
client.Run();

private static void Client_OnCommentRecieved(object sender, WebcastChatMessage e)
{
    Console.WriteLine($"{e.User.uniqueId}: {e.Comment}");
}
````
**This is not an official library nor associated with TikTok in any way.**

## Setup
### Unity
A [Unity package]() can be downloaded from the releases.
### Nuget
The latest release can be found in the Nuget Package Manager or by entering the command:

`Install-Package TikTokLiveSharp`
