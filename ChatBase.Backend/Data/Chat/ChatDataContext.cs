using Microsoft.EntityFrameworkCore;

namespace ChatBase.Backend.Data.Chat
{
    public class ChatDataContext : DbContext
    {
        public virtual DbSet<ChatMessageEntity> ChatMessages { get; set; }
        public ChatDataContext(DbContextOptions<ChatDataContext> options) : base(options)
        {

        }
    }
}
