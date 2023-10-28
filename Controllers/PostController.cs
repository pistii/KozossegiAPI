using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.DTOs.Post;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Runtime.Intrinsics.X86;

namespace KozoskodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class PostController : ControllerBase
    {
        public readonly DBContext _context;
        public PostController(DBContext context)
        {
            _context = context;
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

        //Returns all the whole post with comments
        [HttpGet("GetAll/{postId}")]
        public async Task<List<PostDto>> GetAll(int postId, int currentPage = 1, int itemPerRequest = 10)
        {
            var query = await _context.PersonalPost
                        .Include(p => p.Posts.PostComments)
                        .Where(_ => _.postId == postId).ToListAsync();
            var sortedItems = new List<PostDto>();
            foreach (var item in query)
            {
                //Find the person who made the post
                var person = await _context.Personal.FindAsync(item.personId);
                if (person != null)
                {
                    string fullName = person.firstName + person.middleName + " " + person.lastName;
                    var postComments = item.Posts.PostComments.AsQueryable();
                    string commenterFullName = string.Empty;
                    string commenterAvatar = string.Empty;
                    if (postComments != null)
                    {
                        foreach (var c in postComments)
                        {
                            var author = await _context.Personal.FindAsync(c.FK_AuthorId); //Search the commenter
                            commenterFullName = author.firstName + person.middleName + " " + person.lastName;
                            commenterAvatar = author.avatar;
                        }
                    }
                    sortedItems = query.OrderBy(_ => _.Posts.DateOfPost)
                        .Skip((currentPage - 1) * itemPerRequest)
                        .Take(itemPerRequest)
                        .Select(p => new PostDto //create the post
                        {
                            PersonalPostId = p.personalPostId,
                            FullName = fullName,
                            PostId = p.Posts.Id,
                            AuthorAvatar = person.avatar,
                            AuthorId = person.id,
                            DateOfPost = p.Posts.DateOfPost,
                            PostContent = p.Posts.PostContent,
                            PostComments = p.Posts.PostComments.Select(c => new CommentDto //add comments to it
                            {
                                CommentId = c.commentId,
                                AuthorId = c.FK_AuthorId,
                                CommenterFullName = commenterFullName,
                                CommenterAvatar = commenterAvatar,
                                CommentDate = c.CommentDate,
                                CommentText = c.CommentText
                            }).ToList()
                        })
                        .ToList();
                    return sortedItems;
                }
            }
            return sortedItems;
        }

        [HttpPost]
        [Route("createNew")]
        public async Task<ActionResult<Post>> Post(CreatePostDto dto)
        {
            try
            {
                var user = await _context.Personal.AnyAsync(_ => _.id == dto.userId); //Find the user
                if (user)
                {
                    Post newPost = new Post(); //Create new Post
                    newPost.PostContent = dto.postContent;
                    newPost.DateOfPost = DateTime.UtcNow;
                    newPost.DisLoves = 0;
                    newPost.Loves = 0;

                    _context.Post.Add(newPost);
                    await _context.SaveChangesAsync();

                    //Create new junction table with user and postId
                    PersonalPost personalPost = new PersonalPost()
                    {
                        personId = dto.userId,
                        postId = newPost.Id
                    };
                    _context.PersonalPost.Add(personalPost);
                    await _context.SaveChangesAsync();
                    CreatedAtAction(nameof(Get), new { id = newPost.Id }, newPost);

                    return Ok();
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
        public async Task<IActionResult> Put(int id, Post data)
        {
            try
            {
                var post = await _context.Post.FindAsync(id);
                if (post == null || post.Id != id) 
                    return NotFound("The post id and the given id is incorrect.");

                post.PostComments = null;
                //Modify only the content and the date of post
                post.DateOfPost = DateTime.Now;
                post.PostContent = data.PostContent;
                _context.Entry(post).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return Ok();
                
            } catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //Deletes the post if exists and can be found
        [HttpDelete("{id}")]
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
        [HttpPut("{postId}/{isLikes}/{isIncrement}")]
        public async Task<IActionResult> DoAction(int postId, bool isLikes, bool isIncrement)
        {
            var post = await _context.Post.FindAsync(postId);
            if (post == null) return NotFound();
            //Todo: store also the name of the liker or disliker
            if (isLikes)
            {
                post.Loves = isIncrement ? post.Loves + 1 : post.Loves - 1;
            }
            else
            {
                post.DisLoves = isIncrement ? post.DisLoves + 1 : post.DisLoves - 1;
            }

            _context.Update(post);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
