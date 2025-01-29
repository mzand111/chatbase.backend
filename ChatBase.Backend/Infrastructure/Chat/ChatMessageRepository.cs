using ChatBase.Backend.Data.Chat;
using ChatBase.Backend.Domain.Chat;
using MZBase.EntityFrameworkCore;

namespace ChatBase.Backend.Infrastructure.Chat
{
    public class ChatMessageRepository : LDRCompatibleRepositoryAsync<ChatMessage, ChatMessageEntity, int>, IChatMessageRepository
    {
        private readonly ChatDataContext _context;

        public ChatMessageRepository(ChatDataContext context) : base(context)
        {
            _context = context;
        }
    }
}
