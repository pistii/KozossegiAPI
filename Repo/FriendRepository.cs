using KozossegiAPI.Data;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Repo
{
    public class FriendRepository : GenericRepository<Friend>, IFriendRepository
    {
        private readonly DBContext _context;

        public FriendRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Personal?> GetUserWithNotification(int userId)
        {
            var user = await _context.Personal.Include(p => p.Notifications).FirstOrDefaultAsync(p => p.id == userId);
            return user;
        }

        public async Task<IEnumerable<Personal_IsOnlineDto>> GetAll(int id)
        {
            var users = await _context.Personal.Include(u => u.users)
            .Where(p => _context.Friendship
                .Where(f => (f.FriendId == id || f.UserId == id) && f.StatusId == 1)
                .Select(f => f.UserId == id ? f.FriendId : f.UserId)
                .Contains(p.id)
            ).Select(f => new Personal_IsOnlineDto(f, f.users.isOnlineEnabled))
            .ToListAsync();

            return users;
        }

        public async Task<IEnumerable<Personal>> GetAllFriendAsync(int userId)
        {
            var friends = await _context.Personal.Include(user => user.users)
            .Where(p => _context.Friendship
                .Where(f => (f.FriendId == userId || f.UserId == userId) && f.StatusId == 1)
                .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
                .Contains(p.id)
            )
            .ToListAsync();
            return friends;
        }

        public async Task<IEnumerable<Personal>> GetAllFriendPersonalAsync(int userId)
        {
            var friends = await _context.Personal
            .Where(p => _context.Friendship
                .Where(f => (f.FriendId == userId || f.UserId == userId) && f.StatusId == 1)
                .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
                .Contains(p.id)
            )
            .ToListAsync();
            return friends;
        }


        /// <summary>
        /// Mapper method to get relation between two users.
        /// </summary>
        /// <param name="userA"></param>
        /// <param name="userB"></param>
        /// <returns>Status of the two user.</returns>
        public async Task<UserRelationshipStatus> GetRelationStatusAsync(int userA, int userB)
        {
            var relation = await _context.Friendship
                .FirstOrDefaultAsync(f =>
                    (f.UserId == userA && f.FriendId == userB) ||
                    (f.UserId == userB && f.FriendId == userA));

            if (relation == null)
                return UserRelationshipStatus.Stranger;

            if (relation.StatusId == 1) return UserRelationshipStatus.Friend;
            if (relation.StatusId == 4 && relation.UserId == userA) 
                return UserRelationshipStatus.FriendRequestRejected;
            if (relation.StatusId == 3 && relation.UserId == userA) return UserRelationshipStatus.FriendRequestSent;
            if (relation.StatusId == 5 && relation.FriendId == userA) return UserRelationshipStatus.FriendRequestReceived;

            if (relation.StatusId == 6)
                return UserRelationshipStatus.Blocked;

            return UserRelationshipStatus.Stranger;
            }


        public async Task Delete(Friend request)
        {
            _context.Friendship.Remove(request);
        }

        public async Task Put(Friend_notificationId friendship)
        {
            friendship.FriendshipSince = DateTime.Now;
            await _context.Friendship.AddAsync(friendship);
        }

        public async Task<Friend?> FriendshipExists(Friend friendship)
        {
            return _context.Friendship
                .FirstOrDefault(friend =>
                friend.FriendId == friendship.FriendId && friend.UserId == friendship.UserId ||
                friend.FriendId == friendship.UserId && friend.UserId == friendship.FriendId);
        }

        public async Task<IEnumerable<Personal>> GetAllUserWhoHasBirthdayToday()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            var birthdayUsers = await _context.Personal
                .Where(u => u.DateOfBirth.Value.Month == today.Month &&
                            u.DateOfBirth.Value.Day == today.Day)
                .ToListAsync();
            return birthdayUsers;
        }
    }
}
