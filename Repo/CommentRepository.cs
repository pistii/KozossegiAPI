using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Repo
{
    public class CommentRepository : HelperRepository<Comment>, ICommentRepository<Comment>
    {
        private readonly DBContext _context;
        public CommentRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Post?> GetPostWithComments(int postId)
        {
            var post = await _context.Post
                    .Include(p => p.PostComments)
                    .FirstOrDefaultAsync(p => p.Id == postId);
            return post;
        }
    }
}
