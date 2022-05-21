using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TikTokLiveSharp.Client.Proxy;
using TikTokLiveSharp.Events;
using TikTokLiveSharp.Protobuf;

namespace TikTokLiveSharp.Client
{
    public class TikTokLiveClient : TikTokBaseClient
    {
        public TikTokLiveClient(string uniqueID,
            TimeSpan? timeout = null,
            TimeSpan? pollingInterval = null,
            Dictionary<string, object> clientParams = null,
            bool processInitialData = true,
            bool fetchRoomInfoOnConnect = true,
            bool enableExtendedGiftInfo = true,
            ProxyClientFactory proxyClientFactory = null,
            string lang = "en-US") : base(uniqueID,
                timeout,
                pollingInterval,
                clientParams,
                processInitialData,
                fetchRoomInfoOnConnect,
                enableExtendedGiftInfo,
                proxyClientFactory,
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
            ReadOnlyMemory<byte> bytes = new ReadOnlyMemory<byte>(message.Binary);

            switch (message.Type)
            {
                case nameof(WebcastChatMessage):
                    var chatMessage = ProtoBuf.Serializer.Deserialize<WebcastChatMessage>(bytes);
                    if (OnCommentRecieved != null)
                        this.OnCommentRecieved.Invoke(this, chatMessage);
                    return;
                case nameof(WebcastGiftMessage):
                    var giftMessage = ProtoBuf.Serializer.Deserialize<WebcastGiftMessage>(bytes);
                    if (OnGiftRecieved != null)
                        this.OnGiftRecieved.Invoke(this, giftMessage);
                    return;
                case nameof(WebcastLikeMessage):
                    var likeMessage = ProtoBuf.Serializer.Deserialize<WebcastLikeMessage>(bytes);
                    if (OnLikesRecieved != null)
                        this.OnLikesRecieved.Invoke(this, likeMessage);
                    return;
                case nameof(WebcastQuestionNewMessage):
                    var questionMessage = ProtoBuf.Serializer.Deserialize<WebcastQuestionNewMessage>(bytes);
                    if (OnQuestionRecieved != null)
                        this.OnQuestionRecieved.Invoke(this, questionMessage);
                    return;
                case nameof(WebcastRoomUserSeqMessage):
                    var roomMessage = ProtoBuf.Serializer.Deserialize<WebcastRoomUserSeqMessage>(bytes);
                    if (OnViewerCountUpdated != null)
                        this.OnViewerCountUpdated.Invoke(this, roomMessage);
                    return;
                case nameof(WebcastSocialMessage):
                    var eventMessage = ProtoBuf.Serializer.Deserialize<WebcastSocialMessage>(bytes);
                    this.InvokeSpecialEvent(eventMessage);
                    return;
            }

            if (UnhandledEvent != null)
                this.UnhandledEvent.Invoke(this, EventArgs.Empty);
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

        public event EventHandler<WebcastChatMessage> OnCommentRecieved;
        public event EventHandler<ConnectionEventArgs> OnConnected;
        public event EventHandler<ConnectionEventArgs> OnDisconnected;
        public event EventHandler<WebcastGiftMessage> OnGiftRecieved;
        public event EventHandler<WebcastLikeMessage> OnLikesRecieved;
        public event EventHandler<WebcastQuestionNewMessage> OnQuestionRecieved;
        public event EventHandler<WebcastRoomUserSeqMessage> OnViewerCountUpdated;
        public event EventHandler<User> OnShare;
        public event EventHandler<User> OnFollow;
        public event EventHandler<User> OnJoin;
        public event EventHandler<User> OnLike;
        public event EventHandler<EventArgs> UnhandledEvent;
    }
}
