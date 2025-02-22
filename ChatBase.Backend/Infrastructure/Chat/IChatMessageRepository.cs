using ChatBase.Backend.Data.Chat.Outputs;
using ChatBase.Backend.Domain.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBase.Backend.Infrastructure.Chat
{
    public interface IChatMessageRepository
    {
        Task<List<UserLatestMessages>> GetUserContactsListAsync(string userName);
        Task<List<ChatMessage>> GetWithDate(string who, string with, DateTime Date);
        Task<List<ChatMessage>> GetWithFromId(string who, string with, int? fromID);
        Task<List<ChatMessage>> GetUnreadWithFromUserName(int ID, string fromUserName, string toUserName);
        Task<int> UnReadMessageCount(string UserName);
    }
}
