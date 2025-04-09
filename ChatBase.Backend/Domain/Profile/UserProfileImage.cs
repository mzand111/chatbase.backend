using MZBase.Domain;
using System;

namespace ChatBase.Backend.Domain.Profile;

public class UserProfileImage : Model<Guid>
{
    public Guid UserId { get; set; }
    public byte[]? Image { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid CreatorUserId { get; set; }
}
