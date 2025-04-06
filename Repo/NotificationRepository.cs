using KozossegiAPI.Data;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Realtime;
using KozossegiAPI.Realtime.Connection;
using KozossegiAPI.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Repo
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly DBContext _context;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IMapConnections _connections; 

        public NotificationRepository(
            DBContext context, 
            IHubContext<NotificationHub, INotificationClient> hub,
            IMapConnections mapConnections) : base(context)
        {
            _notificationHub = hub;
            _connections = mapConnections;
            _context = context;
        }



        public async Task RealtimeNotification(int toUserId, GetNotification dto)
        {
            if (_connections.ContainsUser(toUserId))
                foreach (var user in _connections.GetConnectionsById(toUserId))
                {
                    await _notificationHub.Clients.Client(user).ReceiveNotification(toUserId, dto);
                }
        }

        /// <summary>
        //  Search the Person's notifications, and create a new Dto from the inherited notification class
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetNotification>> GetAllNotifications(int userId)
        {
            return await _context.UserNotification
                .Include(n => n.notification)
                .Include(n => n.personal)
                .Where(n => n.UserId == userId).Select(n => new GetNotification(
                        n.NotificationId,
                        n.notification.AuthorId,
                        n.UserId,
                        n.notification.CreatedAt,
                        n.notification.Message,
                        n.IsRead,
                        n.notification.NotificationType,
                        HelperService.GetEnumDescription(n.notification.NotificationType),
                        n.personal.avatar
                    ))
                .ToListAsync();
        }

        public async Task<Notification> CreateFriendRequestAcceptedNotification(CreateNotification createNotification)
        {
            createNotification.NotificationType = NotificationType.FriendRequestAccepted;
            createNotification.Message = string.Empty;
            
            var created = await CreateNotification(createNotification);
            return created;
        }


        public async Task UpdateNotification(CreateNotification cn)
        {
            var newNotification = new Notification(cn.AuthorId, cn.Message, cn.NotificationType);
            await UpdateThenSaveAsync(newNotification);

            UserNotification userNotification = new(cn.UserId, newNotification.Id, false);
            await UpdateThenSaveAsync(userNotification);
        }

        public async Task<Notification> CreateNotification(CreateNotification cn)
        {
            var newNotification = new Notification(cn.AuthorId, cn.Message, cn.NotificationType);
            await InsertSaveAsync(newNotification);

            UserNotification userNotification = new(cn.UserId, newNotification.Id, false);
            await InsertSaveAsync(userNotification);
            return newNotification;
        }
    }
}
