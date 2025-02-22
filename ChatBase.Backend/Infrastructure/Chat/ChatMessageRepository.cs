using ChatBase.Backend.Data.Chat;
using ChatBase.Backend.Data.Chat.Outputs;
using ChatBase.Backend.Domain.Chat;
using Microsoft.EntityFrameworkCore;
using MZBase.EntityFrameworkCore;
using System;
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
        public async Task<List<ChatMessage>> GetWithDate(string who, string with, DateTime Date)
        {
            return await _context.ChatMessages
                 .Where(uu => uu.SendTime.Date == Date.Date &&
                      ((uu.FromUserId.ToLower() == who && uu.ToUserId.ToLower() == with) || (uu.FromUserId.ToLower() == with && uu.ToUserId.ToLower() == who)))
                 .OrderBy(uu => uu.SendTime)
                 .Select(uu => uu.GetDomainObject())
                 .ToListAsync();
        }
        public async Task<List<ChatMessage>> GetWithFromId(string who, string with, int? fromID)
        {
            return await _context.ChatMessages
                    .Where(uu => (uu.ID < fromID || fromID == null) && ((uu.FromUserId.ToLower() == who && uu.ToUserId.ToLower() == with) || (uu.FromUserId.ToLower() == with && uu.ToUserId.ToLower() == who)))
                    .OrderByDescending(uu => uu.SendTime)
                    .Take(10)
                    .Select(uu => uu.GetDomainObject())
                    .ToListAsync();
        }
        public async Task<List<ChatMessage>> GetUnreadWithFromUserName(int ID, string fromUserName, string toUserName)
        {
            return await _context.ChatMessages
                     .Where(uu => uu.ID <= ID && uu.ViewTime == null && uu.FromUserId.ToLower() == fromUserName && uu.ToUserId.ToLower() == toUserName)
                     .Select(uu => uu.GetDomainObject())
                     .ToListAsync();
        }
        public async Task<int> UnReadMessageCount(string UserName)
        {
            return await _context.ChatMessages
                     .Where(a => a.ToUserId.ToLower() == UserName)
                     .Where(a => a.ViewTime == null)
                     .CountAsync();
        }
    }
}
