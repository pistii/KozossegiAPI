using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using KozoskodoAPI.Data;
using Microsoft.EntityFrameworkCore;
using KozoskodoAPI.Repo;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class FriendController : ControllerBase, IFriendRepository
    {
        private readonly DBContext _context;
        public FriendController(DBContext dbContext) {
            _context = dbContext;
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

        [HttpPost] //if user accepts the request
        public async Task<IActionResult> Post(Friend_notificationId friendship)
        {
            try
            {
                _context.Friendship.Add(friendship);

                Notification? notificationModified = await _context.Notification.FindAsync(friendship.NotificationId);
                if (notificationModified != null)
                {
                    notificationModified.isNew = false;
                    notificationModified.notificationContent = "Mostantól ismerősök vagytok.";
                    notificationModified.notificationType = NotificationType.FriendRequestAccepted;
                    _context.Notification.Update(notificationModified);
                }
                //todo: értesíteni a küldő notificationjeit hogy elfogadta a baráti kérelmet
                await _context.SaveChangesAsync();
                return Ok(notificationModified);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
