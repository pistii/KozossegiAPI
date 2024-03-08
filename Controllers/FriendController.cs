using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class FriendController : ControllerBase, IFriendRepository
    {
        private readonly DBContext _context;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IMapConnections _connections;

        public FriendController(DBContext dbContext, IHubContext<NotificationHub, INotificationClient> hub, IMapConnections connections)
        {
            _context = dbContext;
            _notificationHub = hub;
            _connections = connections;
        }

        //For online status
        public async Task<List<Personal>> GetAllFriend(int userId)
        {
            var friends = await _context.Personal
            .Where(p => _context.Friendship
                .Where(f => f.FriendId == userId && f.StatusId == 1)
                .Select(f => f.UserId)
                .Union(_context.Friendship
                    .Where(f => f.UserId == userId && f.StatusId == 1)
                    .Select(f => f.FriendId)
                )
                .Contains(p.id)
            )
            .ToListAsync();

            return friends;
        }

        [HttpGet("{personalId}/{currentPage}/{qty}")]
        public async Task<IActionResult> GetAll(int personalId, int currentPage = 1, int qty = 9)
        {
            var user = await _context.Personal
            .Where(p => _context.Friendship.Include(stat => stat.friendship_status)
                .Where(f => f.FriendId == personalId && f.StatusId == 1)
                .Select(f => f.UserId)
                .Union(_context.Friendship
                    .Where(f => f.UserId == personalId && f.StatusId == 1)
                    .Select(f => f.FriendId)
                )
                .Contains(p.id)
            )
            .ToListAsync();

            var sorted = user?.Skip((currentPage - 1) * qty).Take(qty);
            return Ok(sorted);

        }

        [HttpPost("postFriendRequest")]
        public async Task<IActionResult> postFriendRequest(Notification notification)
        {
            var requestedFromUser = await _context.Personal
                .FirstOrDefaultAsync(_ => _.id == notification.SenderId);
            var requestedUser = await _context.Personal.Include(n => n.Notifications)
                .FirstOrDefaultAsync(_ => _.id == notification.ReceiverId);

            if (requestedUser != null)
            {
                //Kikeressük a korábbi értesítést, ha létezik
                var requestedUsersNotification = requestedUser?.Notifications?.FirstOrDefault(n => n.SenderId == notification.SenderId && n.notificationType == NotificationType.FriendRequest);

                NotificationWithAvatarDto avatarDto = new NotificationWithAvatarDto(
                    notification.ReceiverId,
                    notification.SenderId,
                    requestedFromUser?.avatar,
                    requestedFromUser!.firstName + " " + requestedFromUser.lastName + " ismerősnek jelölt.",
                    notification.notificationType);

                //if (requestedUsersNotification != null)
                //{
                //    //Felülírja a korábbi értesítés dátumát és újként jelöljük meg, ha volt már az adott személytől
                //    requestedUsersNotification.isNew = true;
                //    requestedUsersNotification.createdAt = DateTime.Now;

                //    _context.Notification.Update(requestedUsersNotification);
                //    await _context.SaveChangesAsync();
                //}
                ////Ha nem létezik, létrehozunk egyet.
                //else
                //{
                    notification.notificationContent = avatarDto.notificationContent;
                    requestedUser?.Notifications?.Add(notification);
                    await _context.SaveChangesAsync();
                    avatarDto.notificationId = notification.notificationId;
                //}


                if (_connections.ContainsUser(notification.ReceiverId))
                {
                    //Az user összes létező kapcsolatának kikeresése (mobil/laptop/több böngésző)
                    var AllUserConnection = _connections.keyValuePairs.Where(c => c.Value == notification.ReceiverId).ToList();
                    //Értesítés küldése az összes létező kapcsolat felé.
                    foreach (var item in AllUserConnection)
                    {
                        //await _notificationHub.Clients.User(notification.personId.ToString()).ReceiveNotification(notification.personId, avatarDto);
                        //await _notificationRepository.RealtimeNotification(notification.personId, avatarDto);
                        //await _notificationHub.Clients.Client(item.Key).SendNotification(notification.personId, avatarDto);
                        await _notificationHub.Clients.Client(item.Key).ReceiveNotification(notification.ReceiverId, avatarDto);
                    }
                }
            }
            return Ok("Success");
        }

        [HttpPut("add")] //if user accepts the request
        public async Task<IActionResult> Put(Friend_notificationId friendship)
        {
            var friendshipExist = _context.Friendship
                .FirstOrDefault(friend => 
                friend.FriendId == friendship.FriendId && friend.UserId == friendship.UserId ||
                friend.FriendId == friendship.UserId && friend.UserId == friendship.FriendId);

            if (friendshipExist == null || friendship.FriendId == friendship.UserId) {
                try
                {

                    friendship.FriendshipSince = DateTime.Now;
                    _context.Friendship.Add(friendship);

                    Notification? notificationModified = await _context.Notification.FindAsync(friendship.NotificationId);
                    if (notificationModified != null)
                    {
                        notificationModified.isNew = false;
                        notificationModified.notificationContent = "Mostantól ismerősök vagytok.";
                        notificationModified.notificationType = NotificationType.FriendRequestAccepted;
                        _context.Notification.Update(notificationModified);
                    }

                    await _context.SaveChangesAsync();
                    return Ok(notificationModified);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Friend request)
        {
            var friendship = _context.Friendship.FirstOrDefault(
                f => f.FriendId == request.FriendId && f.UserId == request.UserId ||
                f.UserId == request.FriendId && f.FriendId == request.UserId);
            _context.Friendship.Remove(friendship);
            await _context.SaveChangesAsync();
            return Ok("removed");
        }

        [HttpGet("relation")]
        public async Task<string> GetFamiliarityStatus(int userId, int viewerId)
        {
            if (userId == viewerId)
            {
                return "self";
            }
            var friendship = await _context.Friendship.FirstOrDefaultAsync(
                f => f.FriendId == userId && f.UserId == viewerId ||
                f.FriendId == viewerId && f.UserId == userId);
            if (friendship != null)
            {
                return "friend";
            }
            return "nonfriend";
        }

    }
}
