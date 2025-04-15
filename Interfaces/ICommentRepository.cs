using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using KozossegiAPI.Repo;

namespace KozossegiAPI.Interfaces
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        public Task<CommentDto> Create(int postId, int authorId, string message);
        Task<Comment?> GetCommentByTokenAsync(string token);
        Task<ContentDto<CommentDto>> GetCommentsAsync(string userPublicId, int postId, int currentPage = 1, int itemPerPage = 20);
    }
}
