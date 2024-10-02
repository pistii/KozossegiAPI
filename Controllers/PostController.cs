using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using Microsoft.AspNetCore.Mvc;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Auth.Helpers;

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
            var sortedItems = await _PostRepository.GetImagesAsync(profileId);
            if (sortedItems == null) return null;
            int totalPages = await _PostRepository.GetTotalPages(sortedItems, ipr);

            var returnValue = _PostRepository.Paginator(sortedItems, cp, ipr).ToList();

            return new ContentDto<PostDto>(returnValue, totalPages);
        }

        //[Authorize]
        [HttpPost]
        [Route("new")]
        public async Task<ActionResult<PostDto>> Post([FromForm] CreatePostDto dto)
        {
            //var userFromHeader = (user?)HttpContext.Items["User"];
            //if (userFromHeader == null) return Unauthorized();
            

            var authorUser = await _PostRepository.GetByIdAsync<Personal>(dto.PostAuthor.AuthorId);
            var postedToUser = await _PostRepository.GetByIdAsync<Personal>(dto.PostedToUserId);
            if (authorUser != null)
            {
                var createdPost = await _PostRepository.Create(dto);

                if (dto.FileUpload != null) {
                    await _PostRepository.UploadFile(dto.FileUpload, createdPost);
                }

                //var closerFriends = _chatRepository.GetChatPartenterIds(userFromHeader.userID);

                //TODO: send notification as below
                //foreach (var friendId in closerFriends)
                //{
                //    //TODO: Ötlet, ahelyett hogy mindegyik user táblájához csatolok egy külön értesítést, lehetne egyet, amit vagy külön táblába, vagy külön rekorddal kezelve követve EGYSZER mentenék el. Ezáltal egy "feliratkozási" tulajdonságot készítve. 
                //    if (friendId != 0)
                //    {
                //        NotificationWithAvatarDto notificationWithAvatarDto =
                //            new NotificationWithAvatarDto(
                //                friendId,
                //                authorUser.id,
                //                authorUser.avatar,
                //                dto.postContent,
                //                NotificationType.NewPost);
                //        await _InotificationRepository.RealtimeNotification(friendId, notificationWithAvatarDto);

                //        await _PostRepository.InsertSaveAsync<Notification>(notificationWithAvatarDto);
                //    }
                //}
                //await _PostRepository.SaveAsync();

                PostDto postDto = new PostDto(authorUser, postedToUser.id, createdPost);
                return Ok(postDto);
            }
            return NotFound();
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