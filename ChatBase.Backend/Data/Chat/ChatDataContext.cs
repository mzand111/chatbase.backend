using ChatBase.Backend.Data.Profile;
using Microsoft.EntityFrameworkCore;

namespace ChatBase.Backend.Data.Chat
{
    public class ChatDataContext : DbContext
    {
        public virtual DbSet<ChatMessageEntity> ChatMessages { get; set; }
        public ChatDataContext(DbContextOptions<ProfileDbContext> options) : base(options)
        {

        }
    }
}
