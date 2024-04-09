using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;

namespace KozoskodoAPI.Repo
{
    public interface IPostRepository<T1> : IGenericRepository<T1>
    {
        Task<List<PostDto>> GetAllPost(int profileId, int userId);
        Task<Post?> GetPostWithCommentsById(int postId);
    }
}
