using ChatBase.Backend.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBase.Backend.Domain.Identity
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        private readonly UserStore<ApplicationUser, IdentityRole<Guid>, ApplicationDbContext, Guid, IdentityUserClaim<Guid>,
      IdentityUserRole<Guid>, IdentityUserLogin<Guid>, IdentityUserToken<Guid>, IdentityRoleClaim<Guid>>
      _store;

        public ApplicationUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _store = (UserStore<ApplicationUser, IdentityRole<Guid>, ApplicationDbContext, Guid, IdentityUserClaim<Guid>,
          IdentityUserRole<Guid>, IdentityUserLogin<Guid>, IdentityUserToken<Guid>, IdentityRoleClaim<Guid>>)store;
        }

        public async Task<bool> IsInRoleByUserIdAsync(Guid userId, string roleName)
        {
            var role = _store.Context.Roles.FirstOrDefault(r => r.NormalizedName == roleName.ToUpper());
            if (role == null)
            {
                throw new InvalidOperationException($"Role '{roleName}' not found");
            }

            return _store.Context.UserRoles.Any(uu => uu.UserId == userId && uu.RoleId == role.Id);
        }

    }
}
