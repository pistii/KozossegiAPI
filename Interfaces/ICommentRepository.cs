using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using KozossegiAPI.Repo;

namespace KozossegiAPI.Interfaces
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        public Task<CommentDto> Create(int postId, int authorId, string message);
        Task<Comment> GetByTokenAsync(string commentToken);
        Task<List<CommentDto>> GetCommentsAsync(int postId);
    }
}
