using FirebaseAdmin.Auth;
using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;

namespace KozossegiAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : BaseController<CommentController>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        public CommentController(
            ICommentRepository commentRepository,
            IPostRepository postRepository
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
        public async Task<IActionResult> GetComments(string postToken, int cp)
        {
            var user = GetUser();
            var post = await _postRepository.GetPostByTokenAsync(postToken);
            if (post == null) return NotFound();


            var comments = await _commentRepository.GetCommentsAsync(user.PublicId, post.Posts.Id, cp);

            return Ok(comments);
        }


        //Searches the post by Id and adds a comment to it
        [HttpPost("create")]
        public async Task<IActionResult> Post(CreateCommentDto comment)
        {
            user author = GetUser();
            var post = await _postRepository.GetPostByTokenAsync(comment.PostToken);
            if (author == null || post == null) return NotFound();

            CommentDto newComment = await _commentRepository.Create(post.Posts.Id, author.userID, comment.Message);
            return Ok(newComment);
        }

        [HttpDelete("delete/{token}")]
        public async Task<IActionResult> Delete(string token)
        {
            var commentExist = await _commentRepository.GetByPublicIdAsync<Comment>(token);
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
            user author = GetUser();
            
            var existing = await _commentRepository.GetByPublicIdAsync<Comment>(comment.CommentToken);
            if (existing == null) return NotFound();
            if (existing.FK_AuthorId != author.userID) return Unauthorized();

            existing.LastModified = DateTime.Now;
            existing.CommentText = comment.Message;
            await _commentRepository.UpdateThenSaveAsync(existing);
            
            var personalData = await _postRepository.GetByIdAsync<Personal>(author.userID);
            var dto = new CommentDto(existing, personalData);
            return Ok(dto);
        }



        [HttpGet("like/{commentId}")]
        public async Task<IActionResult> LikeComment(string commentId)
        {
            var user = GetUser();
            string payload = "";

            var comment = await _commentRepository.GetCommentByTokenAsync(commentId);
            if (comment == null) return NotFound();

            var existingReaction = comment.CommentReactions.FirstOrDefault(p => p.FK_UserId == user.userID);

            if (existingReaction == null)
            {
                CommentReaction reaction = new(comment.commentId, user.userID, ReactionType.Like);
                await _commentRepository.InsertSaveAsync(reaction);
            }
            else if (existingReaction.ReactionTypeId == (int)ReactionType.Dislike)
            {
                existingReaction.ReactionTypeId = (int)ReactionType.Like;
                await _commentRepository.UpdateThenSaveAsync(existingReaction);
                payload = "like";
            }
            else
            {
                await _commentRepository.RemoveThenSaveAsync(existingReaction);
                payload = "remove";
            }
            return Ok(payload);
        }


        [HttpGet("dislike/{commentId}")]
        public async Task<IActionResult> DislikePost(string commentId)
        {
            var user = GetUser();
            string payload = "";

            var comment = await _commentRepository.GetCommentByTokenAsync(commentId);
            if (comment == null) return NotFound();

            var existingReaction = comment.CommentReactions.FirstOrDefault(p => p.FK_UserId == user.userID);
            if (existingReaction == null)
            {
                CommentReaction reaction = new(comment.commentId, user.userID, ReactionType.Dislike);
                await _commentRepository.InsertSaveAsync(reaction);
            }
            else if (existingReaction.ReactionTypeId == (int)ReactionType.Like)
            {
                existingReaction.ReactionTypeId = (int)ReactionType.Dislike;
                await _commentRepository.UpdateThenSaveAsync(existingReaction);
                payload = "dislike";
            }
            else
            {
                await _commentRepository.RemoveThenSaveAsync(existingReaction);
                payload = "remove";
            }
            return Ok(payload);
        }
    }
}
