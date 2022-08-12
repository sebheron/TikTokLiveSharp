## Currently not working.
Due to system changes, this current version of TikTokLiveSharp does not work. I'm aiming to get a release out as soon as possible. Any one interested in helping can fork the "Fix-(WIP)" branch and contribute!

# TikTokLiveSharp
![HitCount](https://hits.dwyl.com/sebheron/TikTokLiveSharp.svg?style=flat)
![Issues](https://img.shields.io/github/issues/sebheron/TikTokLiveSharp)
![Forks](https://img.shields.io/github/forks/sebheron/TikTokLiveSharp)
![Stars](https://img.shields.io/github/stars/sebheron/TikTokLiveSharp)
![Tweet](https://img.shields.io/twitter/url?url=https%3A%2F%2Fgithub.com%2Fsebheron%2FTikTokLiveSharp)

#### Read TikTok Live chat messages, gifts, etc.

## Showcase
If you want to showcase a project you've made with this library, create an Issue with the **Showcase** label.

## Details
A C# port of TikTok Live connector library (See [here](https://github.com/zerodytrash/TikTok-Live-Connector), [here](https://github.com/isaackogan/TikTokLive) and [here](https://github.com/Davincible/gotiktoklive) for more in-depth documentation).
The primary incentive behind designing this library was to allow direct implementation of the TikTok Live connector into Unity, it's implemented in .NET Standard and should work universally across all .NET supported platforms. An older version of Protobuf-net was used to ensure Unity compatibility.
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
A [Unity package](https://github.com/sebheron/TikTokLiveSharp/releases/tag/v0.1.2.1) can be downloaded from the releases.
With Unity projects, replace usage of the TikTokLiveClient's Run method with Start to prevent blocking.
### Nuget
The latest release can be found in the Nuget Package Manager or by entering the command:

`Install-Package TikTokLiveSharp`

Or by navigating to the Nuget [URL](https://www.nuget.org/packages/TikTokLiveSharp/0.1.2).
