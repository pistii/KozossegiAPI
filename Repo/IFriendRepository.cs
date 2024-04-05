using KozoskodoAPI.Controllers;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Repo
{
    public interface IFriendRepository : IGenericRepository<Friend>
    {
        Task<IEnumerable<Personal>> GetAll(int id);
        Task<IEnumerable<Personal>> GetAllFriendAsync(int id);
        public Task<string> CheckIfUsersInRelation(int userId, int viewerId);
        public Task Delete(Friend request);
        public Task Put(Friend_notificationId friendship);
        //public Task<List<Personal>> GetAllFriend(int userId);
        public Task<Friend?> FriendshipExists(Friend friendship);
        Task<IEnumerable<Personal>> GetAllUserWhoHasBirthdayToday();
        Task<Friend?> GetByIdAsync(int id);
    }
}
