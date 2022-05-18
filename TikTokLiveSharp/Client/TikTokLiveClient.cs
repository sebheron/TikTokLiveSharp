using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TikTokLiveSharp.Client.Proxy;
using TikTokLiveSharp.Events;
using TikTokLiveSharp.Protobuf;

namespace TikTokLiveSharp.Client
{
    internal class TikTokLiveClient : TikTokBaseClient
    {
        public TikTokLiveClient(string userID,
            bool debugMode = false,
            TimeSpan? timeout = null,
            TimeSpan? pollingInterval = null,
            Dictionary<string, object> clientParams = null,
            Dictionary<string, string> headers = null,
            bool processInitialData = true,
            bool fetchRoomInfoOnConnect = true,
            bool enableExtendedGiftInfo = true,
            ProxyContainer proxyContainer = null,
            string lang = "en-US") : base(userID,
                timeout,
                pollingInterval,
                clientParams,
                headers,
                processInitialData,
                fetchRoomInfoOnConnect,
                enableExtendedGiftInfo,
                proxyContainer,
                lang)
        { }

        public TikTokLiveClient(string userID, bool debugMode = false) : base(userID)
        {
        }

        protected override async Task<string> connect()
        {
            var roomID = await base.connect();
            if (this.Connected)
            {
                this.OnConnection.Invoke(this, new ConnectionEventArgs(true));
            }
            return roomID;
        }

        protected override async Task disconnect()
        {
            await base.disconnect();
            if (!this.Connected)
            {
                this.OnConnection.Invoke(this, new ConnectionEventArgs(false));
            }
        }

        protected override void handleWebcastMessages(WebcastResponse webcastResponse)
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
                    this.OnCommentRecieved.Invoke(this, new CommentEventArgs(chatMessage.User.uniqueId, chatMessage.Comment));
                    break;
                case nameof(WebcastGiftMessage):
                    var giftMessage = ProtoBuf.Serializer.Deserialize<WebcastGiftMessage>(bytes);
                    this.OnGiftRecieved.Invoke(this, new GiftEventArgs(giftMessage.User.uniqueId, giftMessage));
                    break;
            }
        }

        public event EventHandler<CommentEventArgs> OnCommentRecieved;
        public event EventHandler<ConnectionEventArgs> OnConnection;
        public event EventHandler<GiftEventArgs> OnGiftRecieved;
        public event EventHandler<LikeEventArgs> OnLikesRecieved;
        public event EventHandler<QuestionEventArgs> OnQuestionRecieved;
        public event EventHandler<ViewerCountEventArgs> OnViewerCountUpdated;
        public event EventHandler<UserEventArgs> OnLiveShared;
        public event EventHandler<UserEventArgs> OnNewFollower;
        public event EventHandler<UserEventArgs> OnJoin;
        public event EventHandler<EventArgs> UnhandledEvent;
    }
}
