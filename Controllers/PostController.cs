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
                    else if (!string.IsNullOrEmpty(dto.Type) && dto.File != null)
                    {
                        return BadRequest("Invalid file format.");
                    }
                }

                //Create new junction table with user and postId
                PersonalPost personalPost = new PersonalPost()
                {
                    personId = userFromHeader.userID,
                    postId = newPost.Id
                };
                await _PostRepository.InsertSaveAsync<PersonalPost>(personalPost);

                var closerFriends = _chatRepository.GetChatPartenterIds(userFromHeader.userID);
                var stringLength = dto.postContent.Length > 60 ? dto.postContent.Take(50) + "..." : dto.postContent;

                foreach (var friendId in closerFriends)
                {
                    //TODO: Ötlet, ahelyett hogy mindegyik user táblájához csatolok egy külön értesítést, lehetne egyet, amit vagy külön táblába, vagy külön rekorddal kezelve követve EGYSZER mentenék el. Ezáltal egy "feliratkozási" tulajdonságot készítve. 
                    if (friendId != 0)
                    {
                        NotificationWithAvatarDto notificationWithAvatarDto =
                            new NotificationWithAvatarDto(
                                friendId,
                                authorUser.id,
                                authorUser.avatar,
                                stringLength,
                                NotificationType.NewPost);
                        await _InotificationRepository.RealtimeNotification(friendId, notificationWithAvatarDto);

                        await _PostRepository.InsertSaveAsync<Notification>(notificationWithAvatarDto);
                    }
                }
                await _PostRepository.SaveAsync();

                var postToReturn = new PostDto()
                {
                    PostContent = newPost.PostContent,
                    PostId = newPost.Id,
                    AuthorId = dto.SourceId,
                    AuthorAvatar = authorUser.avatar,
                    Dislikes = 0,
                    Likes = 0,
                    FullName = HelperService.GetFullname(authorUser.firstName, authorUser.middleName, authorUser.lastName),
                    DateOfPost = DateTime.Now,
                    PostComments = new(),
                };

                if (!string.IsNullOrEmpty(dto.Name) && dto.File != null)
                {
                    postToReturn.MediaContent = new MediaContent()
                    {
                        MediaType = dto.Type,
                        FileName = dto.Name,
                        Id = postToReturn.PostId,
                    };
                }
                return Ok(postToReturn);
            }
            return NotFound();
        }
    

        //Gets the postId and the modified content of the post
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] CreatePostDto data)
        {
                var post = await _PostRepository.GetByIdAsync<Post>(id);
                if (post == null || post.Id != id)
                    return NotFound();

                //Modify only the content and the date of post
                post.PostContent = data.postContent;
                post.DateOfPost = DateTime.UtcNow;
                await _PostRepository.UpdateThenSaveAsync(post);
                return Ok();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _PostRepository.GetByIdAsync<Post>(id);
            if (post == null)
                return NotFound();

            //Find the person in the personalPost junction table and remove the connection
            var personalPost = await _PostRepository.GetByIdAsync<PersonalPost>(post.Id);
            await _PostRepository.RemoveAsync<PersonalPost>(personalPost);
            await _PostRepository.RemoveAsync(post);
            await _PostRepository.SaveAsync();
            return Ok();
        }

        //Likes or dislikes a post.
        //Also increments or decrements the number of likes or dislikes
        /*
        [HttpPut("action")]
        public async Task<IActionResult> DoAction(Like_DislikeDto dto)
        {
            //TODO: This will be updated when the writing the tests are done. Make it work with comments also from commentcontroller.

            var post = await _PostRepository.GetByIdAsync<Post>(dto.postId);
            if (post == null) return NotFound();
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

        */

    }
}