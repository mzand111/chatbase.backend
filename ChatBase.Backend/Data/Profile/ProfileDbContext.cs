using ChatBase.Backend.Domain.Profile;
using Microsoft.EntityFrameworkCore;

namespace ChatBase.Backend.Data.Profile
{
    public class ProfileDbContext : DbContext
    {
        public virtual DbSet<UserProfileImage> UserProfileImages { get; set; }
        public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options)
        {

        }
    }
}
