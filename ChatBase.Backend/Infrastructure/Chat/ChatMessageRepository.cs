using ChatBase.Backend.Data.Chat;
using ChatBase.Backend.Data.Chat.Outputs;
using ChatBase.Backend.Domain.Chat;
using Microsoft.EntityFrameworkCore;
using MZBase.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBase.Backend.Infrastructure.Chat
{
    public class ChatMessageRepository : LDRCompatibleRepositoryAsync<ChatMessage, ChatMessageEntity, int>, IChatMessageRepository
    {
        private readonly ChatDataContext _context;

        public ChatMessageRepository(ChatDataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<UserLatestMessages>> GetUserContactsListAsync(string userName)
        {
            var loweredUserName = userName.ToLower();
            var messages = await _context.ChatMessages
                .Where(m => m.FromUserId.ToLower() == loweredUserName || m.ToUserId.ToLower() == loweredUserName)
                .GroupBy(m => m.FromUserId.ToLower() == loweredUserName ? m.ToUserId : m.FromUserId)
                .Select(g => new UserLatestMessages
                {
                    UserName = g.Key,
                    LastMessageBody = g.OrderByDescending(m => m.SendTime).FirstOrDefault().Body,
                    LastMessageType = g.OrderByDescending(m => m.SendTime).FirstOrDefault().Type,
                    LastMessageTime = g.OrderByDescending(m => m.SendTime).FirstOrDefault().SendTime
                })
                .ToListAsync();

            return messages;
        }
    }
}
