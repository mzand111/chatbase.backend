using ChatBase.Backend.Domain.Chat;
using MZBase.Infrastructure;

namespace ChatBase.Backend.Data.Chat
{
    public class ChatMessageEntity : ChatMessage, IConvertibleDBModelEntity<ChatMessage>
    {
        public ChatMessage GetDomainObject() => this;

        public void SetFieldsFromDomainModel(ChatMessage domainModelEntity)
        {
            ID = domainModelEntity.ID;
            FromUserId = domainModelEntity.FromUserId;
            ToUserId = domainModelEntity.ToUserId;
            Body = domainModelEntity.Body;
            MetaData = domainModelEntity.MetaData;
            Type = domainModelEntity.Type;
            SendTime = domainModelEntity.SendTime;
            ReceiveTime = domainModelEntity.ReceiveTime;
            ViewTime = domainModelEntity.ViewTime;
            ReplyToID = domainModelEntity.ReplyToID;
            ForwardID = domainModelEntity.ForwardID;
            ForwardDetails = domainModelEntity.ForwardDetails;
            ForwardedFromGroup = domainModelEntity.ForwardedFromGroup;
        }

    }
}
