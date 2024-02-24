using Google.Api;
using Google.Cloud.Storage.V1;
using KozoskodoAPI.Controllers.Cloud;
using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase, IImageRepository
    {
        private StorageController _storageController;
        private DBContext _ctx;
        public ImageController(StorageController storageController)
        {
            _storageController = storageController;
            _ctx = _storageController._context;
        }

        //No use anymore
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostImage(int postId)
        {
            var content = await _ctx.MediaContent.FirstOrDefaultAsync(c => c.MediaContentId == postId);
            var imgName = content.FileName;

            var image = _storageController.GetFile(content.FileName!, StorageController.BucketSelector.IMAGES_BUCKET_NAME).Result;

            return image;
        }

        [HttpGet("getAll/{userId}")]
        public async Task<List<PostDto>> GetAll(int userId, int currentPage = 1, int requestItems = 9) //Todo: implements from interface, it is not tested. Currentpage is recently added 1.22 after disabled gcloud 
        {
            var postsWithImage = await _ctx.PersonalPost
                .Include(p => p.Posts.MediaContents)
                .Include(p => p.Posts.PostComments)
                .Where(p => p.Posts.SourceId == userId && p.Posts.MediaContents != null) //Ennyiben tér el a sima post kérelemtől, hogy még ezt kell vizsgálni
                .OrderByDescending(_ => _.Posts.DateOfPost)
                .Take(requestItems)
                .Select(p => new PostDto
                { 
                    PersonalPostId = p.personalPostId,
                    FullName = $"{p.Personal_posts.firstName} {p.Personal_posts.middleName} {p.Personal_posts.lastName}",
                    PostId = p.Posts.Id,
                    AuthorAvatar = p.Personal_posts.avatar!,
                    AuthorId = p.Personal_posts.id,
                    Likes = p.Posts.Likes,
                    Dislikes = p.Posts.Dislikes,
                    DateOfPost = p.Posts.DateOfPost,
                    PostContent = p.Posts.PostContent!,
                    PostComments = p.Posts.PostComments
                        .Select(c => new CommentDto
                        {
                            CommentId = c.commentId,
                            AuthorId = c.FK_AuthorId,
                            CommenterFirstName = _ctx.Personal.First(_ => _.id == c.FK_AuthorId).firstName!,
                            CommenterMiddleName = _ctx.Personal.First(_ => _.id == c.FK_AuthorId).middleName!,
                            CommenterLastName = _ctx.Personal.First(_ => _.id == c.FK_AuthorId).lastName!,
                            CommenterAvatar = _ctx.Personal.First(_ => _.id == c.FK_AuthorId).avatar!,
                            CommentDate = c.CommentDate,
                            CommentText = c.CommentText!
                        })
                        .ToList(),
                    MediaContents = p.Posts.MediaContents.ToList()
                })
                .ToListAsync();

            return postsWithImage;
        }

        [HttpPost("upload/avatar")]
        public async Task<IActionResult> Upload([FromForm] AvatarUpload fileUpload)
        {
            var res = await _storageController.AddFile(fileUpload, StorageController.BucketSelector.AVATAR_BUCKET_NAME);
            if (res != null)
            {
                await UpdateDatabaseImageUrl(fileUpload.UserId, res);
                return Ok();
            }
            else
            {
                return BadRequest("Something went wrong");
            }
        }


        [HttpPut]
        private async Task UpdateDatabaseImageUrl(int userId, string url)
        {
            var user = await _ctx.Personal.FirstOrDefaultAsync(u => u.id == userId);
            user.avatar = url;
            await _ctx.SaveChangesAsync();
        }
    }
}
