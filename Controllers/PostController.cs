using Humanizer;
using KozoskodoAPI.Controllers.Cloud;
using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Runtime.Intrinsics.X86;
using KozoskodoAPI.Repo;

namespace KozoskodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class PostController : ControllerBase
    {
        public readonly DBContext _context;
        public StorageController? _storageController;
        public INotificationRepository _InotificationRepository;
        public IPostRepository<PostDto> _PostRepository;
        public PostController(DBContext context, 
            StorageController? storageController,
            INotificationRepository notificationRepository,
            IPostRepository<PostDto> postRepository)
        {
            _context = context;
            _storageController = storageController;
            _InotificationRepository = notificationRepository;
            _PostRepository = postRepository;
        }

        public PostController(IPostRepository<PostDto> postRepository)
        {
            _PostRepository = postRepository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> Get(int id)
        {
            //var res = await _context.Post.FindAsync(id);
            var res = _PostRepository.GetByIdAsync<Post>(id).Result;
            if (res != null)
            {
                return Ok(res);
            }
            return NotFound();
        }


        /// <summary>
        /// After authentication and checking the relation between the post viewer and the author of the post and the publicity of the post returns the actual posts.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet("GetAllPost/{profileId}/{userId}/{currentPage}/")]
        [HttpGet("GetAllPost/{profileId}/{userId}/{currentPage}/{itemPerRequest}/")]
        public async Task<ContentDto<PostDto>> GetAllPost(int profileId, int userId, int currentPage = 1, int itemPerRequest = 10)
         {
            var sortedItems = _PostRepository.GetAllPost(profileId, userId, currentPage, itemPerRequest);
            int totalPages = _PostRepository.GetTotalPages(sortedItems.Result, itemPerRequest).Result;
            //var returnValue = sortedItems
            //.Skip((currentPage - 1) * itemPerRequest)
            //.Take(itemPerRequest).ToList();
            var returnValue = _PostRepository.Paginator(sortedItems.Result, currentPage, itemPerRequest).ToList();

            return new ContentDto<PostDto>(returnValue, totalPages);
        }

        [HttpPost]
        [Route("createNew")]
        public async Task<ActionResult<Post>> Post([FromForm] CreatePostDto dto)
        {
            try
            {
                //var user = await _context.Personal.FirstOrDefaultAsync(_ => _.id == dto.userId);
                var user = await _PostRepository.GetByIdAsync<Personal>(dto.userId);
                if (user != null)
                {
                    Post newPost = new()
                    {
                        SourceId = dto.SourceId,
                        PostContent = dto.postContent,
                        Dislikes = 0,
                        Likes = 0
                    };

                    //_context.Post.Add(newPost);
                    //await _context.SaveChangesAsync();
                    await _PostRepository.InsertSaveAsync<Post>(newPost);

                    ContentType type = 
                        dto.Type == "image/jpg" || 
                        dto.Type == "image/jpeg" || 
                        dto.Type == "image/png" ? 
                        ContentType.Image : ContentType.Video;

                    
                    if (dto.File != null)
                    {
                        MediaContent media = new(newPost.Id, dto.Name, type); //mentés az adatbázisba
                        FileUpload newFile = new FileUpload(dto.Name, dto.Type, dto.File); //mentés a felhőbe
                        
                        var fileName = await _storageController.AddFile(newFile, StorageController.BucketSelector.IMAGES_BUCKET_NAME); //Csak a fájl neve tér vissza
                        media.FileName = fileName.ToString();
                        //_context.MediaContent.Add(media);
                        _PostRepository.InsertAsync<MediaContent>(media);
                        //await _context.SaveChangesAsync(); Elég a végén menteni most még...
                    }

                    //Create new junction table with user and postId
                    PersonalPost personalPost = new PersonalPost()
                    {
                        personId = dto.userId,
                        postId = newPost.Id
                    };

                    //_context.PersonalPost.Add(personalPost);
                    //await _context.SaveChangesAsync();
                    await _PostRepository.InsertSaveAsync<PersonalPost>(personalPost);


                    //var closerFriends = _context.ChatRoom.Where(
                    //    u => u.senderId == dto.userId || u.receiverId == dto.userId)
                    //    .Select(f => f.senderId == dto.userId ? f.receiverId : f.senderId).ToList();

                    var closerFriends = _PostRepository.GetCloserFriendIds(dto.userId).Result;
                    var stringLength = dto.postContent.Length > 60 ? dto.postContent.Take(50) + "..." : dto.postContent;

                    foreach (var friendId in closerFriends)
                    {
                        NotificationWithAvatarDto notificationWithAvatarDto = 
                            new NotificationWithAvatarDto(
                                friendId, 
                                user.id, 
                                user.avatar, 
                                dto.postContent, 
                                NotificationType.NewPost);
                        await _InotificationRepository.RealtimeNotification(friendId, notificationWithAvatarDto);
                        //_context.Notification.Add(notificationWithAvatarDto);
                        await _PostRepository.InsertAsync(notificationWithAvatarDto);
                    }
                    await _PostRepository.SaveAsync();

                    return Ok(newPost);
                }
                return BadRequest("Wrong userId");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Gets the postId and the modified content of the post
        [HttpPut("{id}")] 
        public async Task<IActionResult> Put(int id, [FromForm] CreatePostDto data)
        {
            try
            {
                //var post = await _context.Post.FindAsync(id);
                var post = await _PostRepository.GetByIdAsync<Post>(id);
                if (post == null || post.Id != id) 
                    return NotFound("The post id and the given id is incorrect.");

                post.PostComments = null;
                //Modify only the content and the date of post
                post.PostContent = data.postContent;
                //_context.Entry(post).State = EntityState.Modified;
                //await _context.SaveChangesAsync();
                await _PostRepository.InsertSaveAsync(post);
                return Ok("Sikeres módosítás");
                
            } catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //Deletes the post if exists and can be found
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id) 
        {
            //var post = await _context.Post.FindAsync(id);
            var post = _PostRepository.GetByIdAsync<Post>(id).Result;
            if (post == null)
                return NotFound();

            //Find the person in the personalPost junction table and remove the connection
            //var personalPost = _context.PersonalPost.FirstOrDefault(_ => _.postId == post.Id);
            var personalPost = _PostRepository.GetByIdAsync<PersonalPost>(post.Id).Result;
            //_context.PersonalPost.Remove(personalPost);
            await _PostRepository.RemoveAsync<PersonalPost>(personalPost);
            await _PostRepository.RemoveAsync(post);
            await _PostRepository.SaveAsync();
            //_context.Post.Remove(post);
            //await _context.SaveChangesAsync();

            return NoContent();
        }

        //Likes or dislikes a post.
        //Also increments or decrements the number of likes or dislikes
        [HttpPut("action")]
        public async Task<IActionResult> DoAction(Like_DislikeDto dto)
        {
            var post = await _context.Post.FindAsync(dto.postId);
            if (post == null) return NotFound("Something went wrong...");
            //TODO: Refactor into postDto and return also the userReaction
            var isUserDidAction = await _context.PostReaction.FirstOrDefaultAsync(u => u.PostId == dto.postId && u.UserId == dto.UserId);

            if (isUserDidAction == null)
            {
                PostReaction newReaction = new PostReaction()
                {
                    PostId = dto.postId,
                    UserId = dto.UserId,
                    ReactionType = dto.actionType
                };

                _context.PostReaction.Add(newReaction); //Creates a new entry and updates the post
                if (dto.actionType == "like")
                {
                    post.Likes++;
                }
                else if (dto.actionType == "dislike")
                {
                    post.Dislikes++;
                }
            }
            else
            {
                if (isUserDidAction.ReactionType == dto.actionType)
                { //Ha már ugyanaz az a művelet mint korábban akkor törlés
                    _context.PostReaction.Remove(isUserDidAction);
                    if (isUserDidAction.ReactionType == "like")
                    {
                        post.Likes--;
                    }
                    else if (isUserDidAction.ReactionType == "dislike")
                    {
                        post.Dislikes--;
                    }
                }
                if (isUserDidAction.ReactionType != dto.actionType)  //Ha már volt reakció, de más, akkor update
                {
                    isUserDidAction.ReactionType = dto.actionType == "like" ? "dislike" : "like";
                    _context.PostReaction.Update(isUserDidAction);
                    if (isUserDidAction.ReactionType == "like")
                    {
                        post.Likes--;
                        post.Dislikes++;
                    }
                    else if (isUserDidAction.ReactionType == "dislike")
                    {
                        post.Dislikes--;
                        post.Likes++;
                    }
                }
            }

            _context.Post.Update(post);
            await _context.SaveChangesAsync();
            return Ok(post);
        }

        
    }

}
