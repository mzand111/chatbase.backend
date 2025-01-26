using ChatBase.Backend.Domain.Profile;
using MZBase.Infrastructure;
using System;
using System.Threading.Tasks;

namespace ChatBase.Backend.Infrastructure.Profile;

public interface IProfileRepository : IRepositoryAsync<UserProfileImage, Guid>
{
    Task SaveChangesAsync();
}
