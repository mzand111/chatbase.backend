using ChatBase.Backend.Data.Profile;
using ChatBase.Backend.Domain.Profile;
using MZBase.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ChatBase.Backend.Infrastructure.Profile;

public class ProfileRepository : RepositoryAsync<UserProfileImage, UserProfileImageEntity, Guid>, IProfileRepository
{
    public ProfileRepository(ProfileDbContext context) : base(context)
    {
    }

    public async Task SaveChangesAsync()
    => await _context.SaveChangesAsync();
}
