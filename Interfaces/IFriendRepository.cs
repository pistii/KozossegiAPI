﻿using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface IFriendRepository : IGenericRepository<Friend>
    {
        Task<IEnumerable<Personal_IsOnlineDto>> GetAll(int id);
        Task<IEnumerable<Personal>> GetAllFriendAsync(int id);
        public Task<string> CheckIfUsersInRelation(int userId, int viewerId);
        public Task Delete(Friend request);
        public Task Put(Friend_notificationId friendship);
        //public Task<List<Personal>> GetAllFriend(int userId);
        public Task<Friend?> FriendshipExists(Friend friendship);
        Task<IEnumerable<Personal>> GetAllUserWhoHasBirthdayToday();
        Task<Personal?> GetUserWithNotification(int userId);
        Task<IEnumerable<Personal>> GetAllFriendPersonalAsync(int userId);
    }
}
