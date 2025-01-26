using ChatBase.Backend.Controllers.Base;
using ChatBase.Backend.Domain.Identity;
using ChatBase.Backend.Infrastructure.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;
using Image = SixLabors.ImageSharp.Image;

namespace ChatBase.Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserProfileImageController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProfileRepository _profileRepo;

        public UserProfileImageController(UserManager<ApplicationUser> userManager, IProfileRepository profileRepo)
        {
            _userManager = userManager;
            _profileRepo = profileRepo;
        }
        [AllowAnonymous]
        [HttpGet("GetUserProfileImageByUserId")]
        public async Task<IActionResult> GetUserProfileImageByUserId(Guid userID, int size = 128)
        {
            var user = await _userManager.FindByIdAsync(userID.ToString());

            if (user.CurrentProfileImageId == null)
            {
                return Ok(null);
            }

            var profileImageRecord = await _profileRepo.FirstOrDefaultAsync(uu => uu.ID == user.CurrentProfileImageId.Value);

            // Retrieve the user's profile image from the database
            byte[] imageBytes = profileImageRecord.Image;

            // Convert the byte array to a Bitmap object
            using (var originalImage = Image.Load<Rgba32>(imageBytes))
            {
                // Resize the image while maintaining the aspect ratio
                int width, height;
                if (originalImage.Width > originalImage.Height)
                {
                    width = size;
                    height = (int)(originalImage.Height * (512.0 / originalImage.Width));
                }
                else
                {
                    height = size;
                    width = (int)(originalImage.Width * (size * 1.0 / originalImage.Height));
                }

                // resize image
                originalImage.Mutate(ctx => ctx.Resize(width, height));

                // Convert the resulting Bitmap object to a byte array and return it
                var memoryStream = new MemoryStream();
                originalImage.SaveAsPng(memoryStream);
                var data = memoryStream.ToArray();
                return File(data, "image/png");
            }
        }
        /// <summary>
        /// Sets profile image of the caller user.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("SetMyProfileImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SetMyProfileImage(IFormFile file = null)
        {
            var user = await _userManager.FindByIdAsync(UserId.ToString());
            string path = "";
            try
            {
                if (file != null && file.Length > 0)
                {
                    if (file.Length > (3000 * 1024))
                    {
                        return BadRequest("File should not be bigger than 3 MBs");
                    }

                    //path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "UploadedFiles"));
                    //if (!Directory.Exists(path))
                    //{
                    //    Directory.CreateDirectory(path);
                    //}
                    using (var dataStream = new MemoryStream())
                    {
                        await file.CopyToAsync(dataStream);
                        var newItemId = Guid.NewGuid();
                        await _profileRepo.InsertAsync(new Domain.Profile.UserProfileImage()
                        {
                            ID = newItemId,
                            CreationTime = DateTime.Now,
                            CreatorUserId = UserId.Value,
                            Image = dataStream.ToArray(),
                            UserId = UserId.Value

                        });
                        await _profileRepo.SaveChangesAsync();
                        user.CurrentProfileImageId = newItemId;
                    }
                    await _userManager.UpdateAsync(user);
                    return Ok();
                }
                else
                {
                    return BadRequest("File is empty");
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error in setting profile image");
                return StatusCode(500, "Error in setting profile image");
            }
        }

        [Authorize]
        [HttpDelete("RemoveMyProfileImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RemoveMyProfileImage()
        {
            var user = await _userManager.FindByIdAsync(UserId.ToString());
            string path = "";
            try
            {
                using (var dataStream = new MemoryStream())
                {
                    user.CurrentProfileImageId = null;
                }
                await _userManager.UpdateAsync(user);
                return Ok();

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error in setting profile image");
                return StatusCode(500, "Error in setting profile image");
            }
        }
    }
}
