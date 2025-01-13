using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatBase.Backend.Controllers.Base
{
    [Authorize]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        public string? UserFirstName
        {
            get
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    if (User.Identity is ClaimsIdentity claimsIdentity)
                    {
                        return claimsIdentity.FindFirst(JwtRegisteredClaimNames.Name)?.Value;

                    }
                }
                return null;

            }
        }
        public Guid? UserId
        {
            get
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    if (User.Identity is ClaimsIdentity claimsIdentity)
                    {
                        var f1 = claimsIdentity.FindFirst("UserId")?.Value;
                        if (f1 != null)
                        {
                            return new Guid(f1);
                        }
                        var f2 = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        return new Guid(f2);
                    }
                }
                return null;

            }
        }
        public string? UserLastName
        {
            get
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    if (User.Identity is ClaimsIdentity claimsIdentity)
                    {
                        return claimsIdentity.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value;
                    }
                }
                return null;

            }
        }
        public string? UserName
        {
            get
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    return User.Identity.Name;
                }
                return null;

            }
        }
    }
}
