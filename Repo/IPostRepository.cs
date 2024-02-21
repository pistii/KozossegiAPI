using KozoskodoAPI.DTOs;

namespace KozoskodoAPI.Repo
{
    public interface IPostRepository
    {
        Task<ContentDto<PostDto>> GetAllPost(int profileId, int userId, int currentPage = 1, int itemPerRequest = 10);
    }
}
