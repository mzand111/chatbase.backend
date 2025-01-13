using ChatBase.Backend.Controllers.Base;
using ChatBase.Backend.Domain.Identity;
using ChatBase.Backend.Domain.Identity.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatBase.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager
            , IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] ServiceLoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && user.RemoveTime == null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, user.FirstName),
                    new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                     new Claim("UserId", user.Id.ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));

                }

                var token = GetToken(authClaims);

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaims(authClaims);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(
                   CookieAuthenticationDefaults.AuthenticationScheme,
                   principal,
                   new AuthenticationProperties
                   {
                       IsPersistent = true,
                       AllowRefresh = true,
                       ExpiresUtc = DateTime.UtcNow.AddDays(1)
                   });
                var userId = user.Id;

                return Ok(new
                AuthorizedUserResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    TokenValidTo = token.ValidTo,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Id = user.Id,
                    RoleNames = userRoles.Count > 0 ? string.Join(userRoles[0], ",") : "",
                    PhoneNumber = user.PhoneNumber,
                });
            }
            return Unauthorized();
        }


        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            int expirationHours = 7 * 24;
            var expirationSetting = _configuration["JWT:TokenExpiration_Hours"];
            if (!string.IsNullOrWhiteSpace(expirationSetting))
            {
                Int32.TryParse(expirationSetting, out expirationHours);
            }
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(expirationHours),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
