using System;
using System.Threading.Tasks;

namespace TikTokLiveSharp.Client.Sockets
{
    public interface ITikTokWebSocket
    {
        /// <summary>
        /// Is the websocket currently connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Connect to the websocket.
        /// </summary>
        /// <param name="url">Websocket url.</param>
        /// <returns>Task to await.</returns>
        Task Connect(string url);

        /// <summary>
        /// Disconnects from the websocket.
        /// </summary>
        /// <returns>Task to await.</returns>
        Task Disconnect();

        /// <summary>
        /// Recieves a message from websocket.
        /// </summary>
        /// <returns></returns>
        Task<TikTokWebSocketResponse> RecieveMessage();

        /// <summary>
        /// Writes a message to the websocket.
        /// </summary>
        /// <param name="arr">The bytes to write.</param>
        /// <returns>Task to await.</returns>
        Task WriteMessage(ArraySegment<byte> arr);
    }
}
