using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using Microsoft.AspNetCore.Mvc;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Repo;

namespace KozossegiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : BaseController<PostController>
    {
        private readonly IPostRepository _postRepository;
        private readonly INotificationRepository _notificationRepository;

        public PostController(
            IPostRepository postRepository,
            INotificationRepository notificationRepository
            )
        {
            _postRepository = postRepository;
            _notificationRepository = notificationRepository;
        }

        [HttpGet("get/{token}")]
        public async Task<ActionResult<Post>> GetByTokenAsync(string token)
        {
            var res = await _postRepository.GetPostByTokenAsync(token);
            if (res != null)
            {
                return Ok(res);
            }
            return NotFound();
        }


        [HttpGet("getAll/{profileId}/{currentPage}/")]
        [HttpGet("getAll/{profileId}/{currentPage}/{itemPerRequest}/")]
        public async Task<IActionResult> GetAllPost(string profileId, int currentPage = 1, int itemPerRequest = 10)
        {
            var userId = GetUserId();
            var publishedToUser = await _postRepository.GetByPublicIdAsync<user>(profileId);
            if (profileId == null) return NotFound("User not found");

            var sortedItems = await _postRepository.GetAllPost(publishedToUser.userID, profileId, currentPage, itemPerRequest);
            return Ok(sortedItems);
        }


        //[HttpGet("getPhotos/{profileId}/{cp}/{ipr}")]
        //public async Task<ContentDto<PostDto>> GetImages(int profileId, int cp = 1, int ipr = 10)
        //{
        //    var sortedItems = await _postRepository.GetImagesAsync(profileId);
        //    if (sortedItems == null) return null;
        //    int totalPages = await _postRepository.GetTotalPages(sortedItems, ipr);

        //    var returnValue = _postRepository.Paginator(sortedItems, cp, ipr).ToList();

        //    return new ContentDto<PostDto>(returnValue, totalPages);
        //}


        [HttpPost("new")]
        public async Task<ActionResult<PostDto>> CreatePost([FromForm] CreatePostDto dto)
        {
            var userId = GetUserId();

            var authorUser = await _postRepository.GetByIdAsync<Personal>(userId);
            user? postedToUser = await _postRepository.GetByPublicIdAsync<user>(dto.PostedToUserId);
            if (postedToUser == null || authorUser == null) return NotFound("Invalid user id");

            bool canPost = await _postRepository.UserCanCreatePost(postedToUser.userID, userId);
            if (!canPost) return BadRequest("You cannot create post for this user.");

            var createdPost = await _postRepository.Create(dto, authorUser, authorUser.id);

            if (dto.FileUpload != null)
            {
                await _postRepository.UploadFile(dto.FileUpload, createdPost);
            }

            //Create and send notification if online
            if (authorUser.id != userId)
            {
                CreateNotification createNotification = new(authorUser.id, postedToUser.userID, NotificationType.NewPost);

                await _notificationRepository.SendNotification(postedToUser.userID, authorUser, createNotification);
            }

            PostDto postDto = new PostDto(authorUser, postedToUser.PublicId, createdPost);
            return Ok(postDto);
        }


        [HttpGet("like/{postToken}")]
        public async Task<IActionResult> LikePost(string postToken)
        {
            var user = GetUser();
            string payload = "";

            var post = await _postRepository.GetPostWithReactionsByTokenAsync(postToken);
            if (post == null) return NotFound();

            var existingReaction = post.PostReactions.FirstOrDefault(p => p.UserId == user.userID);
            
            if (existingReaction == null)
            {
                PostReaction reaction = new(post.Id, user.userID, ReactionType.Like);
                await _postRepository.InsertSaveAsync(reaction);
            }
            else if (existingReaction.ReactionTypeId == (int)ReactionType.Dislike)
            {
                existingReaction.ReactionTypeId = (int)ReactionType.Like;
                await _postRepository.UpdateThenSaveAsync(existingReaction);
                payload = "like";
            }
            else 
            {
                await _postRepository.RemoveThenSaveAsync(existingReaction);
                payload = "remove";
            }
            return Ok(payload);
        }

        [HttpGet("dislike/{postToken}")]
        public async Task<IActionResult> DislikePost(string postToken)
        {
            var user = GetUser();
            string payload = "";

            var post = await _postRepository.GetPostWithReactionsByTokenAsync(postToken);
            if (post == null) return NotFound();

            var existingReaction = post.PostReactions.FirstOrDefault(p => p.UserId == user.userID);
            if (existingReaction == null)
            {
                PostReaction reaction = new(post.Id, user.userID, ReactionType.Dislike);
                await _postRepository.InsertSaveAsync(reaction);
            }
            else if (existingReaction.ReactionTypeId == (int)ReactionType.Like)
            {
                existingReaction.ReactionTypeId = (int)ReactionType.Dislike;
                await _postRepository.UpdateThenSaveAsync(existingReaction);
                payload = "dislike";
            }
            else
            {
                await _postRepository.RemoveThenSaveAsync(existingReaction);
                payload = "remove";
            }
            return Ok(payload);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Put([FromForm] UpdatePostDto data)
        {
            var user = GetUser();

            var post = await _postRepository.GetPostByTokenAsync(data.Token);
            if (post == null) return NotFound();
            if (post.Posts.MediaContent != null) return BadRequest("You cannot upload other file for now.");
            if (post.AuthorId != user.userID) return Unauthorized();

            post.Posts.PostContent = data.Message;
            post.Posts.LastModified = DateTime.Now;

            await _postRepository.UpdateThenSaveAsync(post.Posts);
            return Ok(post.Posts);
        }

        [HttpDelete("delete/{token}")]
        public async Task<IActionResult> Delete(string token)
        {
            var post = await _postRepository.GetPostByTokenAsync(token);
            if (post == null)
                return NotFound();
            else if (post.AuthorId != GetUserId()) return Unauthorized();

            await _postRepository.RemovePostAsync(post.Posts);
            return Ok();
        }
    }
}