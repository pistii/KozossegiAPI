using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.DTOs;
using KozossegiAPI.Repo;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Models.Cloud;
using Microsoft.AspNetCore.Mvc;
using Serilog;

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
                        return Ok();
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


        //No use anymore
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostImage(int postId)
        {
            var content = await _imageRepository.GetPostImage(postId);
            var imgName = content.FileName;

            var image = await _storageRepository.GetFile(content.FileName!, BucketSelector.IMAGES_BUCKET_NAME);

            return image;
        }

        [HttpGet("getAll/{userId}")]
        public async Task<List<PostDto>> GetAll(int userId, int currentPage = 1, int requestItems = 9) //Todo: implements from interface, it is not tested. Currentpage is recently added 1.22 after disabled gcloud 
        {
            var postsWithImage = await _imageRepository.GetAll(userId, currentPage, requestItems);
            return postsWithImage;
        }

    }
}
