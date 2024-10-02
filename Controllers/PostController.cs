using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using Microsoft.AspNetCore.Mvc;
using KozossegiAPI.Interfaces;

namespace KozossegiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class PostController : ControllerBase
    {
        private IStorageRepository? _storageController;
        private INotificationRepository _InotificationRepository;
        private IPostRepository<PostDto> _PostRepository;
        private IChatRepository<ChatRoom, Personal> _chatRepository;
        //private IPostPhotoStorage _postPhotoStorage;

        public PostController(

            IPostRepository<PostDto> postRepository,
            //IPostPhotoStorage postPhotoStorage,
            IStorageRepository? storageController = null,
            INotificationRepository notificationRepository = null,
            IChatRepository<ChatRoom, Personal> chatRepository = null)
        {
            _storageController = storageController;
            _InotificationRepository = notificationRepository;
            _PostRepository = postRepository;
            _chatRepository = chatRepository;
            //_postPhotoStorage = postPhotoStorage;
        }

        [HttpGet("get/{token}")]
        public async Task<ActionResult<Post>> GetByTokenAsync(string token)
        {
            var res = await _PostRepository.GetPostByTokenAsync(token);
            if (res != null)
            {
                return Ok(res);
            }
            return NotFound();
        }


        [HttpGet("getAll/{profileId}/{userId}/{currentPage}/")]
        [HttpGet("getAll/{profileId}/{userId}/{currentPage}/{itemPerRequest}/")]
        public async Task<ContentDto<PostDto>> GetAllPost(int profileId, int userId, int currentPage = 1, int itemPerRequest = 10)
        {
            var sortedItems = await _PostRepository.GetAllPost(profileId, userId, currentPage, itemPerRequest);
            return sortedItems;
        }


        [HttpGet("getPhotos/{profileId}/{cp}/{ipr}")]
        public async Task<ContentDto<PostDto>> GetImages(int profileId, int cp = 1, int ipr = 10)
        {
            var sortedItems = await _PostRepository.GetImages(profileId);
            if (sortedItems == null) return null;
            int totalPages = await _PostRepository.GetTotalPages(sortedItems, ipr);

            var returnValue = _PostRepository.Paginator(sortedItems, cp, ipr).ToList();

            return new ContentDto<PostDto>(returnValue, totalPages);
        }

        [Authorize]
        [HttpPost]
        [Route("createNew")]
        public async Task<ActionResult<Post>> Post([FromForm] CreatePostDto dto)
        {
            var userFromHeader = (user?)HttpContext.Items["User"];
            if (userFromHeader == null) return Unauthorized();
            

            var authorUser = await _PostRepository.GetByIdAsync<Personal>(userFromHeader.userID);
            
            if (authorUser != null)
            {
                Post newPost = new()
                {
                    SourceId = authorUser.id,
                    PostContent = dto.postContent,
                    Dislikes = 0,
                    Likes = 0
                };

                await _PostRepository.InsertSaveAsync<Post>(newPost);

                if (dto.postContent != null)
                {
                    bool isVideo = FileHandlerService.FormatIsVideo(dto.Type);
                    bool isImage = FileHandlerService.FormatIsImage(dto.Type);
                    //If file is accepted format: save. Otherwise return with badRequest
                    if (isVideo || isImage)
                    {
                        if (dto.File != null)
                        {
                            MediaContent media = new(newPost.Id, dto.Name, dto.Type); //mentés az adatbázisba
                            FileUpload newFile = new FileUpload(dto.Name, dto.Type, dto.File); //mentés a felhőbe

                            var fileName = await _storageController.AddFile(newFile, BucketSelector.IMAGES_BUCKET_NAME); //Csak a fájl neve tér vissza
                            media.FileName = fileName.ToString();
                            await _PostRepository.InsertAsync<MediaContent>(media);
                        }
                    }


        [HttpPost("like")]
        [Authorize]
        public async Task<IActionResult> LikePost(ReactionDto reactionDto)
                    {
            var user = (user)HttpContext.Items["User"];
            string payload = "";

            if (reactionDto.Type != "like")
                {
                return BadRequest();
            }

            var post = await _PostRepository.GetPostWithReactionsByTokenAsync(reactionDto.Token);
            if (post == null)
                    {
                return NotFound();
                }

            //Check if user didn't liked the post
            if (!post.PostReactions.Any(p => p.UserId == user.userID))
                {
                await _PostRepository.LikePost(reactionDto, post, user);
            }
            else  //already has a reaction. If dislike, make a like action, othervise should remove the like
                {
                var itemToRemove = post.PostReactions.FirstOrDefault(p => p.UserId == user.userID);
                if (itemToRemove.ReactionTypeId == 2)
                    {
                    await _PostRepository.LikePost(reactionDto, post, user);
                    payload = "liked";
                }
                await _PostRepository.RemoveThenSaveAsync(itemToRemove);
            }
            return Ok(payload);
        }
    
        [HttpPost("dislike")]
        [Authorize]
        public async Task<IActionResult> DislikePost(ReactionDto reactionDto)
        {
            var user = (user)HttpContext.Items["User"];
            string payload = "";

            if (reactionDto.Type != "dislike")
            {
                return BadRequest();
        }

            var post = await _PostRepository.GetPostWithReactionsByTokenAsync(reactionDto.Token);
            if (post == null)
        {
                return NotFound();
        }


            //Check if user didn't disliked the post
            if (!post.PostReactions.Any(p => p.UserId == user.userID))
                {
                await _PostRepository.DislikePost(reactionDto, post, user);
            }
            else //already has a reaction. If like, make a dislike action, othervise should remove the dislike
                    {
                var itemToRemove = post.PostReactions.FirstOrDefault(p => p.UserId == user.userID);
                if (itemToRemove.ReactionTypeId == 1)
                    {
                    await _PostRepository.DislikePost(reactionDto, post, user);
                    payload = "disliked";
                    }
                await _PostRepository.RemoveThenSaveAsync(itemToRemove);
                }
            return Ok(payload);
                    }

        [HttpPut("update")]
        public async Task<IActionResult> Put([FromForm] CreatePostDto data)
                    {
            var post = await _PostRepository.GetPostByTokenAsync(data.post.Token);
            if (post == null)
                return NotFound();

            post.PostContent = data.post.PostContent;
            post.LastModified = DateTime.Now;
            await _PostRepository.UpdateThenSaveAsync(post);
            return Ok();
        }

        [HttpDelete("delete/{token}")]
        public async Task<IActionResult> Delete(string token)
        {
            var post = await _PostRepository.GetPostByTokenAsync(token);
            if (post == null)
                return NotFound();

            await _PostRepository.RemovePostAsync(post);
            return Ok();
        }
    }
}