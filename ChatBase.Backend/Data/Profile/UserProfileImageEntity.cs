using ChatBase.Backend.Domain.Profile;
using MZBase.Infrastructure;

namespace ChatBase.Backend.Data.Profile;

public class UserProfileImageEntity : UserProfileImage, IConvertibleDBModelEntity<UserProfileImage>
{
    public UserProfileImage GetDomainObject() => this;

    public void SetFieldsFromDomainModel(UserProfileImage domainModelEntity)
    {
        this.ID = domainModelEntity.ID;
        this.UserId = domainModelEntity.UserId;
        this.CreatorUserId = domainModelEntity.CreatorUserId;
        this.CreationTime = domainModelEntity.CreationTime;
        this.Image = domainModelEntity.Image;
    }
}
