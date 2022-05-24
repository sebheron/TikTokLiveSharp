using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TikTokLiveSharp.Client.Proxy;
using TikTokLiveSharp.Events;
using TikTokLiveSharp.Protobuf;

namespace TikTokLiveSharp.Client
{
    public class TikTokLiveClient : TikTokBaseClient
    {
        /// <summary>
        /// Creates a new instance of the TikTok Live client.
        /// Used for retrieving a stream of information from live streams.
        /// </summary>
        /// <param name="uniqueID">The unique ID of the user.</param>
        /// <param name="timeout">The timeout to be used with requests.</param>
        /// <param name="pollingInterval">The polling interval to use (The space between requests).</param>
        /// <param name="clientParams">The client parameters available.</param>
        /// <param name="processInitialData">Should the data be processed on connection.</param>
        /// <param name="fetchRoomInfoOnConnect">Should room information be retrieved on connection.</param>
        /// <param name="enableExtendedGiftInfo"></param>
        /// <param name="proxyHandler"></param>
        /// <param name="lang"></param>
        public TikTokLiveClient(string uniqueID,
            TimeSpan? timeout = null,
            TimeSpan? pollingInterval = null,
            Dictionary<string, object> clientParams = null,
            bool processInitialData = true,
            bool fetchRoomInfoOnConnect = true,
            bool enableExtendedGiftInfo = true,
            RotatingProxy proxyHandler = null,
            string lang = "en-US") : base(uniqueID,
                timeout,
                pollingInterval,
                clientParams,
                processInitialData,
                fetchRoomInfoOnConnect,
                enableExtendedGiftInfo,
                proxyHandler,
                lang)
        { }

        protected override async Task<string> Connect()
        {
            var roomID = await base.Connect();
            if (this.Connected && this.OnConnected != null)
            {
                this.OnConnected.Invoke(this, new ConnectionEventArgs(true));
            }
            return roomID;
        }

        protected override async Task Disconnect()
        {
            await base.Disconnect();
            if (!this.Connected && this.OnDisconnected != null)
            {
                this.OnDisconnected.Invoke(this, new ConnectionEventArgs(false));
            }
        }

        protected override void HandleWebcastMessages(WebcastResponse webcastResponse)
        {
            foreach (var message in webcastResponse.Messages)
            {
                this.InvokeEvent(message);
            }
        }

        private void InvokeEvent(Message message)
        {
            using (var stream = new MemoryStream(message.Binary))
            switch (message.Type)
            {
                case nameof(WebcastChatMessage):
                    var chatMessage = ProtoBuf.Serializer.Deserialize<WebcastChatMessage>(stream);
                    if (OnCommentRecieved != null)
                        this.OnCommentRecieved.Invoke(this, chatMessage);
                    return;
                case nameof(WebcastGiftMessage):
                    var giftMessage = ProtoBuf.Serializer.Deserialize<WebcastGiftMessage>(stream);
                    if (OnGiftRecieved != null)
                        this.OnGiftRecieved.Invoke(this, giftMessage);
                    return;
                case nameof(WebcastLikeMessage):
                    var likeMessage = ProtoBuf.Serializer.Deserialize<WebcastLikeMessage>(stream);
                    if (OnLikesRecieved != null)
                        this.OnLikesRecieved.Invoke(this, likeMessage);
                    return;
                case nameof(WebcastQuestionNewMessage):
                    var questionMessage = ProtoBuf.Serializer.Deserialize<WebcastQuestionNewMessage>(stream);
                    if (OnQuestionRecieved != null)
                        this.OnQuestionRecieved.Invoke(this, questionMessage);
                    return;
                case nameof(WebcastRoomUserSeqMessage):
                    var roomMessage = ProtoBuf.Serializer.Deserialize<WebcastRoomUserSeqMessage>(stream);
                    if (OnViewerCountUpdated != null)
                        this.OnViewerCountUpdated.Invoke(this, roomMessage);
                    return;
                case nameof(WebcastSocialMessage):
                    var eventMessage = ProtoBuf.Serializer.Deserialize<WebcastSocialMessage>(stream);
                    this.InvokeSpecialEvent(eventMessage);
                    return;
            }

            if (UnhandledEvent != null)
                this.UnhandledEvent.Invoke(this, message);
        }

        private void InvokeSpecialEvent(WebcastSocialMessage messageEvent)
        {
            switch (messageEvent.Event.eventDetails.displayType)
            {
                case "pm_mt_msg_viewer":
                    if (OnLike != null)
                        this.OnLike.Invoke(this, null);
                    return;
                case "live_room_enter_toast":
                    if (OnJoin != null)
                        this.OnJoin.Invoke(this, messageEvent.User);
                    return;
                case "pm_main_follow_message_viewer_2":
                    if (OnFollow != null)
                        this.OnFollow.Invoke(this, messageEvent.User);
                    return;
                case "pm_mt_guidance_share":
                    if (OnShare != null)
                        this.OnShare.Invoke(this, messageEvent.User);
                    return;
            }
        }

        /// <summary>
        /// Event thrown when comments are recieved.
        /// </summary>
        public event EventHandler<WebcastChatMessage> OnCommentRecieved;
        /// <summary>
        /// Event thrown when the client connects.
        /// </summary>
        public event EventHandler<ConnectionEventArgs> OnConnected;
        /// <summary>
        /// Event thrown when the client disconnects.
        /// </summary>
        public event EventHandler<ConnectionEventArgs> OnDisconnected;
        /// <summary>
        /// Event thrown when a gift is recieved.
        /// </summary>
        public event EventHandler<WebcastGiftMessage> OnGiftRecieved;
        /// <summary>
        /// Event thrown when groups of likes are recieved.
        /// Note: OnLike is more likely to be useful here, as OnLikesRecieved is called infrequently.
        /// </summary>
        public event EventHandler<WebcastLikeMessage> OnLikesRecieved;
        /// <summary>
        /// Event thrown when questions are recieved.
        /// </summary>
        public event EventHandler<WebcastQuestionNewMessage> OnQuestionRecieved;
        /// <summary>
        /// Event thrown when the view count updates.
        /// </summary>
        public event EventHandler<WebcastRoomUserSeqMessage> OnViewerCountUpdated;
        /// <summary>
        /// Event thrown when the stream is shared.
        /// </summary>
        public event EventHandler<User> OnShare;
        /// <summary>
        /// Event thrown when someone follows.
        /// </summary>
        public event EventHandler<User> OnFollow;
        /// <summary>
        /// Event thrown when someone joins the stream.
        /// </summary>
        public event EventHandler<User> OnJoin;
        /// <summary>
        /// Event thrown when someone likes the stream.
        /// </summary>
        public event EventHandler<User> OnLike;
        /// <summary>
        /// Event thrown when an unhandled event is recieved from the webcast.
        /// It's up to you how you can interpret this message.
        /// </summary>
        public event EventHandler<Message> UnhandledEvent;
    }
}
