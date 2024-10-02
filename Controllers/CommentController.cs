using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository<PostDto> _postRepository;
        public CommentController(
            ICommentRepository commentRepository,
            IPostRepository<PostDto> postRepository
            )
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
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

        [HttpGet("get/{postToken}/{cp}")]
        public async Task<IActionResult> GetSome(string postToken, int cp)
        {
            var post = await _postRepository.GetPostByTokenAsync(postToken);
            if (post == null) return NotFound();


            var comments = await _commentRepository.GetCommentsAsync(post.Id);

            var commentsToReturn = _commentRepository.Paginator(comments, cp);
            return Ok(commentsToReturn);
        }


        //Searches the post by Id and adds a comment for it
        [HttpPost]
        [Route("new")]
        public async Task<IActionResult> Post(NewCommentDto comment)
        {
            var user = await _commentRepository.GetByIdAsync<Personal>(comment.commenterId);
            var post = await _postRepository.GetPostByTokenAsync(comment.postToken);
            if (user == null || post == null) return NotFound();

            CommentDto newComment = await _commentRepository.Create(post.Id, user.id, comment.commentTxt);
            return Ok(newComment);
        }

        [HttpDelete("delete/{token}")]
        public async Task<IActionResult> Delete(string token)
        {
            Comment commentExist = await _commentRepository.GetByTokenAsync(token);
            if (commentExist != null)
            {
                await _commentRepository.RemoveThenSaveAsync(commentExist);
                return Ok();
            }
            return NotFound();
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateCommentDto comment)
        {
            if (comment.commenterId == null || 
                string.IsNullOrEmpty(comment.CommentToken)
                )
                return BadRequest("Commenter and token cannot be empty.");

            Comment existing = await _commentRepository.GetByTokenAsync(comment.CommentToken!);
            if (existing == null)
            {
                return NotFound();
            }
            if (existing.FK_AuthorId != comment.commenterId)
            {
                return BadRequest("Mit szeretnél?");
            }

            existing.LastModified = DateTime.Now;
            existing.CommentText = comment.CommentTxt;
            await _commentRepository.UpdateThenSaveAsync(existing);
            return Ok(existing);
        }
    }
}
