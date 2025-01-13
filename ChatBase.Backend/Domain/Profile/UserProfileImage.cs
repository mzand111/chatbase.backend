using System;

namespace ChatBase.Backend.Domain.Profile
{
    public class UserProfileImage
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public byte[]? Image { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid CreatorUserId { get; set; }

    }
}
