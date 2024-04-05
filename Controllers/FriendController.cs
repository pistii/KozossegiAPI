﻿using KozoskodoAPI.Data;
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
    public class FriendController : ControllerBase
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IPersonalRepository<Personal> _personalRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IMapConnections _connections;

        public FriendController(
            IFriendRepository friendRepository,
            IPersonalRepository<Personal> personalRepository,
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

        [HttpPost("postFriendRequest")]
        public async Task<IActionResult> postFriendRequest(Notification notification)
        {
            var requestedFromUser = await _personalRepository.Get(notification.SenderId);
            var requestedUser = await _personalRepository.Get(notification.ReceiverId);

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
                await _friendRepository.SaveAsync();
                avatarDto.notificationId = notification.notificationId;
                //}


                if (_connections.ContainsUser(notification.ReceiverId))
                {
                    //Az user összes létező kapcsolatának kikeresése (mobil/laptop/több böngésző)
                    var AllUserConnection = _connections.keyValuePairs.Where(c => c.Value == notification.ReceiverId).ToList();
                    //Értesítés küldése az összes létező kapcsolat felé.
                    foreach (var item in AllUserConnection)
                    {
                        await _notificationHub.Clients.Client(item.Key).ReceiveNotification(notification.ReceiverId, avatarDto);
                    }
                }
                return Ok("Success");
            }
            return NoContent();
        }

        [HttpPut("add")] //if user accepts or rejects the request
        public async Task<IActionResult> Put(Friend_notificationId friendship)
        {
            var friendshipExist = _friendRepository.FriendshipExists(friendship);

            if (friendshipExist == null || friendship.FriendId != friendship.UserId) { //Utóbbi feltétel azt vizsgálja, hogy ne tudja ismerősnek jelölni saját magát...
                try
                {
                    Notification? notificationModified = _notificationRepository.GetNotification(friendship).Result;
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
                            await _notificationRepository.Update(notificationModified);
                        }
                        await _friendRepository.SaveAsync();
                        return Ok(notificationModified);
                    }
                    else if (friendship.StatusId == 4) //Baráti kérelem elutasítva
                    {
                        await _friendRepository.Delete(friendship);
                        if (notificationModified != null)
                        {
                            notificationModified.notificationContent = "Ismerős kérelem elutasítva.";
                            notificationModified.notificationType = NotificationType.FriendRequestReject;
                            _notificationRepository.Update(notificationModified);
                        }

                        await _friendRepository.SaveAsync();
                        return Ok(notificationModified);
                    }
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
            var friendship = _friendRepository.FriendshipExists(request);
            if (friendship != null)
            {
                await _friendRepository.Delete(friendship.Result!);
                await _friendRepository.SaveAsync();
                return Ok("removed");
            }
            return NoContent();
        }

        [HttpGet("relation")]
        public string GetFamiliarityStatus(int userId, int viewerId)
        {
            string friendship = _friendRepository.CheckIfUsersInRelation(userId, viewerId).Result;
            return friendship;
        }

    }
}