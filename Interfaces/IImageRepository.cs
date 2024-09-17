using KozossegiAPI.DTOs;
using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface IImageRepository
    {
        Task<MediaContent> GetPostImage(int postId);
        Task<List<PostDto>> GetAll(int userId, int currentPage = 1, int requestItems = 9);
        Task UpdateDatabaseImageUrl(int userId, string url);
    }
}
