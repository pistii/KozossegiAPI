using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly IPostRepository<Comment> _commentRepository;
        public CommentController(IPostRepository<Comment> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var res = await _commentRepository.GetByIdAsync<Comment>(id);
            if (res != null)
            {
                return Ok(res);
            }
            return NotFound();
        }

        //Searches the post by Id and adds a comment for it
        [HttpPost]
        [Route("newComment")]
        public async Task<IActionResult> Post(NewCommentDto comment)
        {
            var user = await _commentRepository.GetByIdAsync<Personal>(comment.commenterId);
            var post = await _commentRepository.GetByIdAsync<Post>(comment.postId);
            if (user == null || post == null) return NotFound();

<<<<<<< HEAD
            Comment newComment = new Comment();
            newComment.PostId = post.Id;
            newComment.FK_AuthorId = user.id;
            newComment.CommentDate = DateTime.UtcNow;
            newComment.CommentText = comment.commentTxt;

            await _commentRepository.InsertSaveAsync(newComment);
            return Ok(newComment);
        }
=======
                Comment newComment = new Comment();
                newComment.PostId = post.Id;
                newComment.FK_AuthorId = user.id;
            newComment.CommentDate = DateTime.Now;
                newComment.CommentText = comment.commentTxt;

            await _commentRepository.InsertSaveAsync<Comment>(newComment);
                return Ok(newComment);
            }
>>>>>>> dadf0531cb4743811d424142f1336b430996bf5f

        //Delete a comment
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Comment? comment = await _commentRepository.GetByIdAsync<Comment>(id);
            if (comment == null) return NotFound();
            
            await _commentRepository.RemoveThenSaveAsync(comment);
            return Ok();
        }

        //Modify the comment. Waits for the postId and the modifiable parameters
        [HttpPut("modify/{id}")]
        public async Task<IActionResult> Put(int id, NewCommentDto comment)
        {
            //var post = await _commentRepository.GetPostWithCommentsById(comment.postId);

            //var targetComment = post?.PostComments?.FirstOrDefault(item => item.commentId == id);
<<<<<<< HEAD

            var targetComment = await _commentRepository.GetByIdAsync<Comment>(comment.CommentId);
            if (targetComment == null) return NotFound();
                
            targetComment.CommentDate = DateTime.UtcNow;
            targetComment.CommentText = comment.commentTxt;
=======
                
            var targetComment = await _commentRepository.GetByIdAsync<Comment>(comment.CommentId);
                if (targetComment == null) return NotFound();
                
            targetComment.CommentDate = DateTime.Now;
                targetComment.CommentText = comment.commentTxt;
>>>>>>> dadf0531cb4743811d424142f1336b430996bf5f

            await _commentRepository.UpdateThenSaveAsync(targetComment);

<<<<<<< HEAD
            return Ok();
=======
                return Ok();
>>>>>>> dadf0531cb4743811d424142f1336b430996bf5f
            
        }

        //TODO: Do the post like/dislike function
        //Likes or dislikes a comment.
        //Also increments or decrements the number of likes or dislikes
        [HttpPut("{commentId}/{isLikes}/{isIncrement}")]
        public async Task<IActionResult> DoAction(int commentId, bool isLikes, bool isIncrement)
        {
            //var comment = await _context.Comment.FindAsync(commentId);
            var comment = await _commentRepository.GetByIdAsync<Comment>(commentId);
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
