using ChatBase.Backend.Domain.Chat;
using System.ComponentModel.DataAnnotations;

namespace ChatBase.Backend.Lib.Dto.Chat
{
    public class ChatMessage_Add
    {

        /// <summary>
        /// UserId of the receiver of the message
        /// </summary>
        [StringLength(200)]
        public virtual string ToUserId { get; set; } = string.Empty;
        /// <summary>
        /// Body of the message
        /// </summary>
        [StringLength(2500)]
        public virtual string Body { get; set; } = string.Empty;
        /// <summary>
        /// Any meta-data related to the message
        /// </summary>
        [StringLength(500)]
        public string? MetaData { get; set; }
        /// <summary>
        /// Type of the message
        /// </summary>
        public ChatMessageType Type { get; set; }

        public int? ReplyTo { get; set; }
        public int? ForwardID { get; set; }
        public string? ForwardDetails { get; set; }
        public bool ForwardedFromGroup { get; set; }
    }
}
