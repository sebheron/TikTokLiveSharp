using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TikTokLiveSharp.Client.Proxy;
using TikTokLiveSharp.Events;
using TikTokLiveSharp.Models;

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

        /// <summary>
        /// Event fired when comments are recieved.
        /// </summary>
        public event EventHandler<WebcastChatMessage> OnCommentRecieved;

        /// <summary>
        /// Event fired when the client connects.
        /// </summary>
        public event EventHandler<ConnectionEventArgs> OnConnected;

        /// <summary>
        /// Event fired when the client disconnects.
        /// </summary>
        public event EventHandler<ConnectionEventArgs> OnDisconnected;

        /// <summary>
        /// Event fired when someone follows.
        /// </summary>
        public event EventHandler<User> OnFollow;

        /// <summary>
        /// Event fired when a gift is recieved.
        /// </summary>
        public event EventHandler<WebcastGiftMessage> OnGiftRecieved;

        /// <summary>
        /// Event fired when someone joins the stream.
        /// </summary>
        public event EventHandler<User> OnJoin;

        /// <summary>
        /// Event fired when someone likes the stream.
        /// </summary>
        public event EventHandler<User> OnLike;

        /// <summary>
        /// Event fired when groups of likes are recieved.
        /// Note: OnLike is more likely to be useful here, as OnLikesRecieved is called infrequently.
        /// </summary>
        public event EventHandler<WebcastLikeMessage> OnLikesRecieved;

        /// <summary>
        /// Event fired when questions are recieved.
        /// </summary>
        public event EventHandler<WebcastQuestionNewMessage> OnQuestionRecieved;

        /// <summary>
        /// Event fired when the stream is shared.
        /// </summary>
        public event EventHandler<ShareEventArgs> OnShare;

        /// <summary>
        /// Event fired when the stream is shared to 5 or more users / 10 or more users.
        /// </summary>
        public event EventHandler<ShareEventArgs> OnMoreShare;

        /// <summary>
        /// Event fired when the view count updates.
        /// </summary>
        public event EventHandler<WebcastRoomUserSeqMessage> OnViewerCountUpdated;

        /// <summary>
        /// Event fired when the live has ended.
        /// </summary>
        public event EventHandler OnLiveEnded;

        /// <summary>
        /// Event fired when emotes are recieved.
        /// </summary>
        public event EventHandler<WebcastEmoteChatMessage> OnEmoteRecieved;

        /// <summary>
        /// Event fired when envelopes are recieved.
        /// </summary>
        public event EventHandler<WebcastEnvelopeMessage> OnEnvelopeRecieved;

        /// <summary>
        /// Event fired when a user subscribes.
        /// </summary>
        public event EventHandler<WebcastMemberMessage> OnSubscribe;

        /// <summary>
        /// Event fired when the weekly ranking updates.
        /// </summary>
        public event EventHandler<WebcastHourlyRankMessage> OnWeeklyRankingUpdated;

        /// <summary>
        /// Event fired when a mic battle begins.
        /// </summary>
        public event EventHandler<WebcastLinkMicBattle> OnMicBattle;

        /// <summary>
        /// Event fired during a mic battle when an update is recieved.
        /// </summary>
        public event EventHandler<WebcastLinkMicArmies> OnMicBattleUpdated;

        /// <summary>
        /// Event fired when an unhandled social event is recieved from the webcast.
        /// It's up to you how you can interpret this message.
        /// </summary>
        public event EventHandler<WebcastSocialMessage> UnhandledSocialEvent;

        /// <summary>
        /// Event fired when an unhandled member event is recieved from the webcast.
        /// It's up to you how you can interpret this message.
        /// </summary>
        public event EventHandler<WebcastMemberMessage> UnhandledMemberEvent;

        /// <summary>
        /// Event fired when an unhandled event is recieved from the webcast.
        /// It's up to you how you can interpret this message.
        /// </summary>
        public event EventHandler<Message> UnhandledEvent;

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

                    case nameof(WebcastEmoteChatMessage):
                        var emoteMessage = ProtoBuf.Serializer.Deserialize<WebcastEmoteChatMessage>(stream);
                        if (OnEmoteRecieved != null)
                            this.OnEmoteRecieved.Invoke(this, emoteMessage);
                        return;

                    case nameof(WebcastHourlyRankMessage):
                        var hourlyRankMessage = ProtoBuf.Serializer.Deserialize<WebcastHourlyRankMessage>(stream);
                        if (OnWeeklyRankingUpdated != null)
                            this.OnWeeklyRankingUpdated.Invoke(this, hourlyRankMessage);
                        return;

                    case nameof(WebcastEnvelopeMessage):
                        var envelopeMessage = ProtoBuf.Serializer.Deserialize<WebcastEnvelopeMessage>(stream);
                        if (OnEnvelopeRecieved != null)
                            this.OnEnvelopeRecieved(this, envelopeMessage);
                        return;

                    case nameof(WebcastLinkMicBattle):
                        var linkMicBattle = ProtoBuf.Serializer.Deserialize<WebcastLinkMicBattle>(stream);
                        if (OnMicBattle != null)
                            this.OnMicBattle.Invoke(this, linkMicBattle);
                        return;

                    case nameof(WebcastLinkMicArmies):
                        var linkMicArmies = ProtoBuf.Serializer.Deserialize<WebcastLinkMicArmies>(stream);
                        this.OnMicBattleUpdated(this, linkMicArmies);
                        return;

                    case nameof(WebcastSocialMessage):
                        var socialMessage = ProtoBuf.Serializer.Deserialize<WebcastSocialMessage>(stream);
                        this.InvokeSpecialEvent(socialMessage);
                        return;

                    case nameof(WebcastMemberMessage):
                        var memberMessage = ProtoBuf.Serializer.Deserialize<WebcastMemberMessage>(stream);
                        if (memberMessage.Event.eventDetails?.displayType == "live_room_enter_toast")
                        {
                            if (OnJoin != null)
                                this.OnJoin.Invoke(this, memberMessage.User);
                        }
                        else if (memberMessage.actionId == 7)
                        {
                            if (OnSubscribe != null)
                                this.OnSubscribe.Invoke(this, memberMessage);
                        }
                        else if (UnhandledMemberEvent != null)
                            this.UnhandledMemberEvent.Invoke(this, memberMessage);
                        return;

                    case nameof(WebcastControlMessage):
                        var controlMessage = ProtoBuf.Serializer.Deserialize<WebcastControlMessage>(stream);
                        if (controlMessage.Action == 3)
                            if (OnLiveEnded != null)
                                this.OnLiveEnded.Invoke(this, new EventArgs());
                        return;
                }

            if (UnhandledEvent != null)
                this.UnhandledEvent.Invoke(this, message);
        }

        private void InvokeSpecialEvent(WebcastSocialMessage messageEvent)
        {
            var match = Regex.Match(messageEvent.Event.eventDetails.displayType, "pm_mt_guidance_viewer_([0-9]+)_share");
            if (match.Success)
            {
                if (OnMoreShare != null && int.TryParse(match.Groups[0].Value, out var result))
                    this.OnMoreShare.Invoke(this, new ShareEventArgs(messageEvent.User, result));
                return;
            }

            switch (messageEvent.Event.eventDetails.displayType)
            {
                case "pm_mt_msg_viewer":
                    if (OnLike != null)
                        this.OnLike.Invoke(this, null);
                    return;

                case "pm_main_follow_message_viewer_2":
                    if (OnFollow != null)
                        this.OnFollow.Invoke(this, messageEvent.User);
                    return;

                case "pm_mt_guidance_share":
                    if (OnShare != null)
                        this.OnShare.Invoke(this, new ShareEventArgs(messageEvent.User, 1));
                    return;

                case "pm_mt_join_message_other_viewer":
                    if (OnJoin != null)
                        this.OnJoin.Invoke(this, messageEvent.User);
                    return;
            }

            if (UnhandledSocialEvent != null)
                this.UnhandledSocialEvent.Invoke(this, messageEvent);
        }
    }
}