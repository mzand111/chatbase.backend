using ChatBase.Backend.Controllers.Base;
using ChatBase.Backend.Domain.Identity;
using ChatBase.Backend.Domain.Identity.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBase.Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IdentityController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUserStore<ApplicationUser> _userStore;

        public IdentityController(UserManager<ApplicationUser> userManager
            , IConfiguration configuration,
            IUserStore<ApplicationUser> userStore)
        {
            _userManager = userManager;
            _configuration = configuration;
            _userStore = userStore;
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
                    ProfileImageId = user.CurrentProfileImageId,
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

        /// <summary>
        /// This method will statically return this string: 'Hello from SignalR chat service!'. Anonymous call is allowed. 
        /// </summary>
        /// <remarks>Call this method to check the health of your connection to SignalR chat server</remarks>
        [HttpGet("HeartBeet")]
        [AllowAnonymous]
        public async Task<IActionResult> HeartBeet()
        {
            return Ok("Hello from SignalR chat service!");
        }

        /// <summary>
        /// This method will statically return this string: 'You are authenticated. Hello from SignalR chat service!' when called by an authenticated user. Anonymous call is not allowed. 
        /// </summary>
        /// <remarks>Call this method to check if the user is successfully authenticated by the SignalR chat server</remarks>
        [Authorize]
        [HttpGet("AuthorizedHeartBeet")]
        public async Task<IActionResult> AuthorizedHeartBeet()
        {
            return Ok("You are authenticated. Hello from SignalR chat service!");
        }

        [Authorize]
        [HttpGet("MyProfile")]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.FindByNameAsync(UserName);
            var userRoles = await _userManager.GetRolesAsync(user);
            return Ok(new UserProfileResponse
            {

                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Id = user.Id,
                RoleNames = userRoles.Count > 0 ? string.Join(userRoles[0], ",") : "",
                PhoneNumber = user.PhoneNumber,
                ProfileImageId = user.CurrentProfileImageId
            });
        }

        /// <summary>
        /// Resets the password of the caller user. 
        /// </summary>
        /// <param name="oldPassword">Old password</param>
        /// <param name="newPassword">New password</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("ResetMyPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetMyPassword(string oldPassword, string newPassword)
        {


            var user = await _userManager.FindByIdAsync(UserId.Value.ToString());
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!changePasswordResult.Succeeded)
            {
                string s = "";
                foreach (var error in changePasswordResult.Errors)
                {
                    s += (s.Length > 0 ? ", " : "") + error.Description;
                }

                return BadRequest("Error in resetting password:" + s);
            }
            return Ok();
        }


        /// <summary>
        /// (To be called just by SuperAdmin users): Resets the password of the given user. 
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <param name="newPassword">New password</param>
        /// <returns></returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("AdminResetPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AdminResetPassword(Guid userId, string newPassword)
        {


            ApplicationUser user = null;


            //First error is for old password
            if (userId == Guid.Empty)
            {
                return BadRequest("User id is not valid");
            }
            if (userId == Guid.Empty)
            {
                return BadRequest("User id is not valid");
            }


            user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                string s = "";
                foreach (var error in removePasswordResult.Errors)
                {
                    s += (s.Length > 0 ? ", " : "") + error.Description;
                }

                return BadRequest("Error in removing password:" + s);
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                string s = "";
                foreach (var error in addPasswordResult.Errors)
                {
                    s += (s.Length > 0 ? ", " : "") + error.Description;
                }

                return BadRequest("Error in removing password:" + s);
            }
            return Ok();

        }

        [Authorize]
        [HttpPost("UpdateMyProfile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMyProfile(ProfileUserDto profile)
        {
            ApplicationUser user = null;
            user = await _userManager.FindByIdAsync(UserId.ToString());
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var firstName = user.FirstName;
            var lastName = user.LastName;
            if (profile.FirstName != firstName)
            {
                user.FirstName = profile.FirstName;
                await _userManager.UpdateAsync(user);
            }
            if (profile.LastName != lastName)
            {
                user.LastName = profile.LastName;
                await _userManager.UpdateAsync(user);
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (profile.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, profile.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    var statusMessage = "Unexpected error when trying to set phone number.";
                    return BadRequest(statusMessage);
                }
            }
            return Ok();
        }
        [AllowAnonymous]
        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<Ok, ValidationProblem>> Register(RegisterUserDto profile)
        {
            var user = new ApplicationUser();
            user.FirstName = profile.FirstName;
            user.LastName = profile.LastName;
            user.PhoneNumber = profile.PhoneNumber;
            await _userStore.SetUserNameAsync(user, profile.Email, CancellationToken.None);
            var emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
            await emailStore.SetEmailAsync(user, profile.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, profile.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            //await SendConfirmationEmailAsync(user, _userManager, HttpContext, profile.Email);
            return TypedResults.Ok();
        }
        private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]> {
            { errorCode, [errorDescription] }
        });
        private static ValidationProblem CreateValidationProblem(IdentityResult result)
        {
            // We expect a single error code and description in the normal case.
            // This could be golfed with GroupBy and ToDictionary, but perf! :P
            Debug.Assert(!result.Succeeded);
            var errorDictionary = new Dictionary<string, string[]>(1);

            foreach (var error in result.Errors)
            {
                string[] newDescriptions;

                if (errorDictionary.TryGetValue(error.Code, out var descriptions))
                {
                    newDescriptions = new string[descriptions.Length + 1];
                    Array.Copy(descriptions, newDescriptions, descriptions.Length);
                    newDescriptions[descriptions.Length] = error.Description;
                }
                else
                {
                    newDescriptions = [error.Description];
                }

                errorDictionary[error.Code] = newDescriptions;
            }

            return TypedResults.ValidationProblem(errorDictionary);
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            //if (result.Succeeded)
            //{
            //    await _emailService.SendEmail(user.Email, "BingoApp - Successfully Registered", "Congratulations,\n You have successfully activated your account!\n " +
            //         "Welcome to the dark side.");
            //}


            return null;
        }
        //async Task SendConfirmationEmailAsync(ApplicationUser user, UserManager<ApplicationUser> userManager, HttpContext context, string email, bool isChange = false)
        //{
        //    //if (confirmEmailEndpointName is null)
        //    //{
        //    //    throw new NotSupportedException("No email confirmation endpoint was registered!");
        //    //}

        //    var code = isChange
        //        ? await userManager.GenerateChangeEmailTokenAsync(user, email)
        //        : await userManager.GenerateEmailConfirmationTokenAsync(user);
        //    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        //    var userId = await userManager.GetUserIdAsync(user);
        //    var routeValues = new RouteValueDictionary()
        //    {
        //        ["userId"] = userId,
        //        ["code"] = code,
        //    };

        //    if (isChange)
        //    {
        //        // This is validated by the /confirmEmail endpoint on change.
        //        routeValues.Add("changedEmail", email);
        //    }

        //    var confirmEmailUrl = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValues)
        //        ?? throw new NotSupportedException($"Could not find endpoint named '{confirmEmailEndpointName}'.");

        //    await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
        //}


    }
}
