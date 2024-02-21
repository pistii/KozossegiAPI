using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        public readonly DBContext _context;
        public CommentController(DBContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> Get(int id)
        {
            var res = await _context.Comment.FindAsync(id);
            if (res != null)
            {
                return Ok(res);
            }
            return NotFound();
        }

        //Searches the post by Id and adds a comment for it
        [HttpPost]
        [Route("newComment")]
        public async Task<ActionResult> Post(NewCommentDto comment)
        {
            try
            {
                var user = await _context.Personal.FindAsync(comment.commenterId);
                var post = await _context.Post.FirstOrDefaultAsync(c => c.Id == comment.postId);

                Comment newComment = new Comment();
                newComment.PostId = post.Id;
                newComment.FK_AuthorId = user.id;
                newComment.CommentDate = DateTime.UtcNow;
                newComment.CommentText = comment.commentTxt;

                _context.Comment.Add(newComment);
                await _context.SaveChangesAsync();
                return Ok(newComment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Delete a comment
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Comment? comment = await _context.Comment.FindAsync(id);
            if (comment == null) return NotFound();
            
            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok();
        }

        //Modify the comment. Waits for the postId and the modifiable parameters
        [HttpPut("modify/{id}")]
        public async Task<IActionResult> Put(int id, NewCommentDto comment)
        {
            try
            {
                var post = await _context.Post
                    .Include(p => p.PostComments)
                    .FirstOrDefaultAsync(p => p.Id == comment.postId);

                var targetComment = post?.PostComments?.FirstOrDefault(item => item.commentId == id);

                if (targetComment == null) return NotFound();
                
                targetComment.CommentDate = DateTime.UtcNow;
                targetComment.CommentText = comment.commentTxt;

                _context.Entry(targetComment).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //This will be added later...

        //Likes or dislikes a comment.
        //Also increments or decrements the number of likes or dislikes
        [HttpPut("{commentId}/{isLikes}/{isIncrement}")]
        public async Task<IActionResult> DoAction(int commentId, bool isLikes, bool isIncrement)
        {
            var comment = await _context.Comment.FindAsync(commentId);
            if (comment == null) return NotFound();

            //TODO: add like/dislike options for a comment also update the database tables
            if (isLikes)
            {
                //comment.Loves = isIncrement ? comment. + 1 : comment.Loves - 1;
            }
            else
            {
                //comment.DisLoves = isIncrement ? comment.DisLoves + 1 : comment.DisLoves - 1;
            }

            //_context.Update(post);
            //await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
