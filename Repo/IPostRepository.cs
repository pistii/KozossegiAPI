using KozoskodoAPI.DTOs;

namespace KozoskodoAPI.Repo
{
    public interface IPostRepository<T1> : IGenericRepository<T1>
    {
        Task<List<PostDto>> GetAllPost(int profileId, int userId, int currentPage = 1, int itemPerRequest = 10);
        Task<List<int>> GetCloserFriendIds(int userId);
    }
}
