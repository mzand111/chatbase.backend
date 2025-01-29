using ChatBase.Backend.Domain.Chat;
using System;

namespace ChatBase.Backend.Data.Chat.Outputs
{
    public class UserLatestMessages
    {

        public string UserName { get; set; }
        public string LastMessageBody { get; set; }
        public ChatMessageType LastMessageType { get; set; }
        public DateTimeOffset LastMessageTime { get; set; }
    }
}
