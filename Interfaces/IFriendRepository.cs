using KozossegiAPI.Models;
using KozossegiAPI.Services;

namespace KozossegiAPI.Interfaces
{
    public interface IFriendRepository : IGenericRepository<Friend>
    {
        Task<IEnumerable<Personal_IsOnlineDto>> GetAll(int id);
        Task<IEnumerable<Personal>> GetAllFriendAsync(int id);
        Task<IEnumerable<Personal>> GetFriendsForInitialUserData(int userId);
        public Task<Friend?> FriendshipExists(Friend friendship);
        Task<Personal?> GetUserWithNotification(int userId);
        Task<UserRelationshipStatus> GetRelationStatusAsync(int userA, int userB);
        void Delete(Friend request);
    }
}
