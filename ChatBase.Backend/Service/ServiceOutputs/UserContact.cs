using ChatBase.Backend.Domain.Chat;
using System;

namespace ChatBase.Backend.Service.ServiceOutputs
{
    public class UserContact
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string DisplayTitle { get; set; }
        public bool IsOnline { get; set; }
        public string? LastMessage { get; set; }
        public DateTimeOffset? LastMessageTime { get; set; }
        public ChatMessageType? LastMessageType { get; set; }
    }
}
