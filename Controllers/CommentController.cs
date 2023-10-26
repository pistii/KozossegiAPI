using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs.Post;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;

namespace KozoskodoAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CommentController<T> : ControllerBase, IController<Comment>
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Comment comment = await _context.Comment.FindAsync(id);
            if (comment != null)
            {
                _context.Comment.Remove(comment);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Comment comment)
        {
            try
            {
                if (id != comment.commentId)
                {
                    _context.Comment.Update(comment);
                    _context.Entry(comment).State = EntityState.Modified;
                    return Ok();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public Task<ActionResult> Post(int id, Comment data)
        {
            throw new NotImplementedException();
        }
    }
}
