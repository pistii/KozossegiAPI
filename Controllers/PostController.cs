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
    public class PostController : ControllerBase, IPostRepository
    {
        public readonly DBContext _context;
        public StorageController? _storageController;
        public PostController(DBContext context, StorageController? storageController)
        {
            _context = context;
            _storageController = storageController;
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> Get(int id)
        {
            var res = await _context.Post.FindAsync(id);
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
            var sortedItems = await _context.PersonalPost
                .Include(p => p.Posts.MediaContents)
                .Include(p => p.Posts.PostComments)
                .Where(p => p.Posts.SourceId == profileId)
                .OrderByDescending(_ => _.Posts.DateOfPost)
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
                            CommenterFirstName = _context.Personal.First(_ => _.id == c.FK_AuthorId).firstName!,
                            CommenterMiddleName = _context.Personal.First(_ => _.id == c.FK_AuthorId).middleName!,
                            CommenterLastName = _context.Personal.First(_ => _.id == c.FK_AuthorId).lastName!,
                            CommenterAvatar = _context.Personal.First(_ => _.id == c.FK_AuthorId).avatar!,
                            CommentDate = c.CommentDate,
                            CommentText = c.CommentText!
                        })
                        .ToList(),
                    MediaContents = p.Posts.MediaContents.ToList(),
                    userReaction = _context.PostReaction.SingleOrDefault(u => p.Posts.Id == u.PostId && u.UserId == userId).ReactionType
                })
                .ToListAsync();


            var totalItems = sortedItems.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / itemPerRequest);

            var returnValue = sortedItems
            .Skip((currentPage - 1) * itemPerRequest)
            .Take(itemPerRequest).ToList();

            return new ContentDto<PostDto>(returnValue, totalPages);
        }

        [HttpPost]
        [Route("createNew")]
        public async Task<ActionResult<Post>> Post([FromForm] CreatePostDto dto)
        {
            try
            {
                var user = await _context.Personal.AnyAsync(_ => _.id == dto.userId);
                if (user)
                {
                    Post newPost = new()
                    {
                        SourceId = dto.SourceId,
                        PostContent = dto.postContent,
                        Dislikes = 0,
                        Likes = 0
                    };

                    _context.Post.Add(newPost);
                    await _context.SaveChangesAsync();

                    ContentType type = 
                        dto.Type == "image/jpg" || 
                        dto.Type == "image/jpeg" || 
                        dto.Type == "image/png" ? 
                        ContentType.Image : ContentType.Video;

                    
                    if (dto.File != null)
                    {
                        MediaContent media = new() //mentés az adatbázisba
                        {
                            MediaContentId = newPost.Id, //Index és egyedi kulcs, ami a post Id-jére fog referálni
                            FileName = dto.Name,
                            ContentType = type,
                        };

                        FileUpload newFile = new FileUpload() //mentés a felhőbe
                        {
                            Name = dto.Name,
                            Type = dto.Type,
                            File = dto.File                            
                        };
                        
                        var fileName = await _storageController.AddFile(newFile, StorageController.BucketSelector.IMAGES_BUCKET_NAME); //Csak a fájl neve tér vissza
                        media.FileName = fileName.ToString();
                        _context.MediaContent.Add(media);

                        //await _context.SaveChangesAsync(); Elég a végén menteni most még...
                    }

                    //Create new junction table with user and postId
                    PersonalPost personalPost = new PersonalPost()
                    {
                        personId = dto.userId,
                        postId = newPost.Id
                    };

                    _context.PersonalPost.Add(personalPost);
                    await _context.SaveChangesAsync();

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
                var post = await _context.Post.FindAsync(id);
                if (post == null || post.Id != id) 
                    return NotFound("The post id and the given id is incorrect.");

                post.PostComments = null;
                //Modify only the content and the date of post
                post.PostContent = data.postContent;
                _context.Entry(post).State = EntityState.Modified;

                await _context.SaveChangesAsync();
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
            var post = await _context.Post.FindAsync(id);
            if (post == null)
                return NotFound();

            //Find the person in the personalPost junction table and remove the connection
            var personalPost = _context.PersonalPost.FirstOrDefault(_ => _.postId == post.Id);
            _context.PersonalPost.Remove(personalPost);
            _context.Post.Remove(post);
            await _context.SaveChangesAsync();

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
