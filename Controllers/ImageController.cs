using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.DTOs;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Models.Cloud;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using KozossegiAPI.Interfaces;

namespace KozossegiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private StorageRepository _storageRepository;
        private IImageRepository _imageRepository;

        public ImageController(StorageRepository storageRepository, 
            IImageRepository imageRepository)
        {
            _storageRepository = storageRepository;
            _imageRepository = imageRepository;
        }


        [Authorize]
        [HttpPost("upload/avatar")]
        public async Task<IActionResult> Upload([FromForm] AvatarUpload fileUpload)
        {

            if (fileUpload.File != null)
            {
                bool isValid = FileHandlerService.FormatIsValid(fileUpload.File.ContentType);
                if (isValid)
                {
                    try
                    {
                        string url = await _storageRepository.AddFile(fileUpload, BucketSelector.AVATAR_BUCKET_NAME);
                        await _imageRepository.UpdateDatabaseImageUrl(fileUpload.UserId, url);
                        return Ok(url);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error on avatar upload. (P1:file, P2:userId)", fileUpload.File.ContentType, fileUpload.UserId);
                        return null;
                    }
                }
                return ValidationProblem("Format not accepted");
            }
            return NotFound();
        }
    }
}
