using Humanizer;
using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Realtime;
using KozoskodoAPI.Realtime.Connection;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IMapConnections _connections;
        private readonly INotificationRepository _notificationRepository;
        private readonly IFriendRepository _friendRepository;



        public NotificationController(
          IHubContext<NotificationHub, INotificationClient> hub, 
          IMapConnections mapConnections,
          INotificationRepository notificationRepository,
          IFriendRepository friendRepository)
        {
            _notificationHub = hub;
            _connections = mapConnections;
            _notificationRepository = notificationRepository;
            _friendRepository = friendRepository;
        }

        [HttpGet("{userId}/{currentPage}")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAll(int userId, int currentPage = 1, int itemPerRequest = 10)
        {
            var notifications = _notificationRepository.GetAll_PersonNotifications(userId).Result;
                        
            //sort, and return the elements
            var few = notifications.OrderByDescending(item => item.createdAt)
                .Skip((currentPage - 1) * itemPerRequest)
                .Take(itemPerRequest);

            if (notifications.Count > 0) return Ok(few);
            
            return NotFound();
        }

        [HttpGet("notificationRead/{notificationId}")]
        public async Task<IActionResult> NotificationReaded(int notificationId)
        {
            try
            {
                var result = _notificationRepository.Get(notificationId).Result;
                if (result.isNew)
                {
                    result.isNew = false;
                    await _notificationRepository.Update(result);
                    await _notificationRepository.SaveAsync();
                    return Ok(result);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
                
        /// <summary>
        /// Searches the users who celebrates the birthday and sends a notification to the user's friends.
        /// </summary>
        /// <returns></returns>
        public async Task BirthdayNotification()
        {
            var birthdayUsers = _friendRepository.GetAllUserWhoHasBirthdayToday().Result;

            foreach (var person in birthdayUsers)
            {
                //Friend Id-k kigyűjtése. StatusId = 1: ha barátok
                //var friendIds = await _context.Friendship
                //    .Where(f => f.UserId == person.id && f.StatusId == 1 || f.FriendId == person.id && f.StatusId == 1)
                //    .Select(f => f.UserId == person.id && f.StatusId == 1 ? f.FriendId : f.UserId)
                //    .ToListAsync();
                var friendIds = await _friendRepository.GetAll(person.id); //TODO: tesztelésre vár

                //Formázás
                string personName = person.middleName == "" ? person.firstName + " " + person.lastName : person.firstName + " " + person.middleName + " " + person.lastName;
                string birthdayContent = personName + " ma ünnepli a születésnapját.";

                foreach (var friend in friendIds)
                {
                    NotificationWithAvatarDto notification = new NotificationWithAvatarDto(friend.id, person.id, person.avatar, birthdayContent, NotificationType.Birthday);

                    //Ha online a felhasználó akkor hubon keresztül értesítjük.
                    await RealtimeNotification(friend.id, notification);

                    await _notificationRepository.InsertAsync(notification);
                    await _notificationRepository.SaveAsync();
                }
            }
        }
        
        
        public async Task SelectNotification()
        {
            var totalNotification = _notificationRepository.DeletableNotifications().Result;

            if (totalNotification.Count() > 1)
            {
                await _notificationRepository.RemoveNotifications(totalNotification);
                await _notificationRepository.SaveAsync();
            }
        }

        public async Task RealtimeNotification(int toUserId, NotificationWithAvatarDto dto) 
        {
            await _notificationHub.Clients.Client(_connections.GetConnectionById(toUserId)).ReceiveNotification(toUserId, dto);
        }    
    }
}
