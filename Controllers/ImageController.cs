using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.DTOs;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Models.Cloud;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;

namespace KozossegiAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : BaseController<ImageController>
    {
        private StorageRepository _storageRepository;
        private IImageRepository _imageRepository;

        public ImageController(StorageRepository storageRepository, 
            IImageRepository imageRepository)
        {
            _storageRepository = storageRepository;
            _imageRepository = imageRepository;
        }


        [HttpPost("upload/avatar")]
        public async Task<IActionResult> Upload([FromForm] AvatarUpload fileUpload)
        {
            var userId = GetUserId();
            if (fileUpload.File != null)
            {
                bool isValid = FileHandlerService.FormatIsValid(fileUpload.File.ContentType);
                if (isValid)
                {
                    try
                    {
                        string url = await _storageRepository.AddFile(fileUpload, BucketSelector.AVATAR_BUCKET_NAME);
                        await _imageRepository.UpdateDatabaseImageUrl(userId, url);
                        return Ok(url);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error on avatar upload. (P1:file, P2:userId)", fileUpload.File.ContentType, userId);
                        return null;
                    }
                }
                return ValidationProblem("Format not accepted");
            }
            return NotFound();
        }
    }
}
