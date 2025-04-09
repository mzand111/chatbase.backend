using ChatBase.Backend.Data.Profile;
using ChatBase.Backend.Domain.Profile;
using MZBase.Infrastructure;
using System;

namespace ChatBase.Backend.Infrastructure.Profile;

public interface IProfileRepository : IRepositoryAsync<UserProfileImage, UserProfileImageEntity, Guid>
{

}
