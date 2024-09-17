using KozossegiAPI.DTOs;
using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface IPostRepository<T1> : IGenericRepository<T1>
    {
        Task<List<PostDto>> GetAllPost(int profileId, int userId);
        Task<List<PostDto>> GetImages(int userId);
        Task<Post?> GetPostWithCommentsById(int postId);
        Task<ContentDto<PostDto>> GetAllPost(int profileId, int userId, int currentPage = 1, int itemPerRequest = 10);
    }
}
