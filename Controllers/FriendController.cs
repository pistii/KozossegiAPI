using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Realtime;
using KozossegiAPI.Realtime.Connection;
using KozossegiAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class FriendController : ControllerBase
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IPersonalRepository _personalRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IMapConnections _connections;

        public FriendController(
            IFriendRepository friendRepository,
            IPersonalRepository personalRepository,
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub, INotificationClient> hub,
            IMapConnections connections)
        {
            _friendRepository = friendRepository;
            _personalRepository = personalRepository;
            _notificationRepository = notificationRepository;
            _notificationHub = hub;
            _connections = connections;

        }

        /// <summary>
        /// Tartalmazza az user táblát, ezt nem szabad vissza küldeni a kliensnek!
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<Personal>> GetAllFriend(int userId)
        {
            var friends = _friendRepository.GetAllFriendAsync(userId).Result.ToList();
            return friends;
        }

        [HttpGet("getFriends/{userId}/{currentPage}/{qty}")]
        public async Task<List<Personal>> GetFriends(int userId)
        {
            var friends = await _friendRepository.GetAllFriendAsync(userId);
            return friends.ToList();
        }

        /// <summary>
        /// Általános lekérdezésnek
        /// </summary>
        /// <param name="personalId"></param>
        /// <param name="currentPage"></param>
        /// <param name="qty"></param>
        /// <returns></returns>
        [HttpGet("{personalId}/{currentPage}/{qty}")]
        public async Task<IActionResult> GetAll(int personalId, int currentPage = 1, int qty = 9)
        {
            var users = await _friendRepository.GetAll(personalId);
            var some = users?.Skip((currentPage - 1) * qty).Take(qty);

            return Ok(some);
        }

        [HttpPost("create/friendRequest")]
        public async Task<IActionResult> CreateFriendRequest(CreateNotification notification)
        {
            var author = await _friendRepository.GetUserWithNotification(notification.AuthorId);
            var receiver = await _friendRepository.GetUserWithNotification(notification.UserId);

            if (receiver == null || author == null) return NotFound("Invalid user id.");
            else if (notification.NotificationType != NotificationType.FriendRequest) return BadRequest("Invalid request");

            //Ellenőrizzük, hogy a küldő user nem kapott-e már barátkérelmet.
            var authorReceivedRequestBefore = author!.UserNotification
               .FirstOrDefault(n => n.UserId == notification.AuthorId &&
               n.notification.NotificationType == NotificationType.FriendRequest);

            Notification createdNotification = new();
            //Létezik korábbi barátkérelem.
            if (authorReceivedRequestBefore != null)
                {
                createdNotification = await _notificationRepository.CreateFriendRequestAcceptedNotification(notification);
                await _friendRepository.MakeThemFriend(notification);
                }
                else
                {
                notification.Message = "";
                createdNotification = await _notificationRepository.CreateNotification(notification);
                }

            // Értesítjük az usert hubon keresztül.
            if (_connections.ContainsUser(notification.UserId))
                {
                //Az user összes létező kapcsolatának kikeresése.
                var allUserConnection = _connections.keyValuePairs.Where(c => c.Value == notification.UserId).ToList();
                //Kigyűjtjük a kapcsolati kulcsokat
                List<string> keys = (from kvp in allUserConnection select kvp.Key).ToList();

                //Az elküldendő objektum
                var noti = new GetNotification(createdNotification.Id, author.id, receiver.id, createdNotification.CreatedAt,
                    "", false, NotificationType.FriendRequest, "New friend request.", author.avatar);
                    //Értesítés küldése az összes létező kapcsolat felé.
                await _notificationHub.Clients.Clients(keys).ReceiveNotification(notification.UserId, noti);
                    }
            return Ok();
                }

        [HttpPut("add")] //if user accepts or rejects the request
        public async Task<IActionResult> Put(Friend_notificationId friendship)
        {
            var friendshipExist = _friendRepository.FriendshipExists(friendship);

            if (friendshipExist == null || friendship.FriendId != friendship.UserId) { //Utóbbi feltétel azt vizsgálja, hogy ne tudja ismerősnek jelölni saját magát...
                try
                {
                    //Notification? notificationModified = _notificationRepository.GetNotification(friendship).Result;
                    Notification? notificationModified = await _notificationRepository.GetByIdAsync<Notification>(friendship.NotificationId);
                    if (notificationModified != null)
                    {
                        notificationModified.isNew = false;
                    }

                    if (friendship.StatusId == 1) //Baráti kérelem érkezett
                    {
                        await _friendRepository.Put(friendship);
                        
                        if (notificationModified != null)
                        {
                            notificationModified.notificationContent = "Mostantól ismerősök vagytok.";
                            notificationModified.notificationType = NotificationType.FriendRequestAccepted;
                            await _notificationRepository.UpdateAsync(notificationModified);
                        }
                        await _friendRepository.SaveAsync();
                        return Ok(notificationModified);
                    }
                    else if (friendship.StatusId == 4) //Baráti kérelem elutasítva
                    {
                        if (notificationModified != null)
                        {
                            notificationModified.notificationContent = "Ismerős kérelem elutasítva.";
                            notificationModified.notificationType = NotificationType.FriendRequestReject;
                            await _notificationRepository.UpdateAsync(notificationModified);
                        }

                        await _friendRepository.Delete(friendship);
                        await _friendRepository.SaveAsync();
                        return Ok(notificationModified);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest();
                }
            }
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Friend request)
        {
            var friendship = await _friendRepository.FriendshipExists(request);
            if (friendship != null)
            {
                await _friendRepository.Delete(friendship);
                await _friendRepository.SaveAsync();
                return Ok("removed");
            }
            return NoContent();
        }

        [HttpGet("relation")]
        public async Task<string> GetFamiliarityStatus(int userId, int viewerId)
        {
            string friendship = await _friendRepository.GetUserRelation(userId, viewerId);
            return friendship;
        }

    }
}
