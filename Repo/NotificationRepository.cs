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


        public async Task SendNotification(int receiverUserId, Personal author, CreateNotification createNotification)
        {
            var notification = new Notification(createNotification.AuthorId, "", createNotification.NotificationType, 
                author.users!.PublicId,
                author.avatar ?? "", 
                createNotification.UserId);
            await InsertSaveAsync(notification);

            GetNotification getNotification = new(notification);
            await _notificationHub.Clients.User(receiverUserId.ToString())
                .ReceiveNotification(receiverUserId, getNotification);
        }

        /// <summary>
        //  Search the Person's notifications, and create a new Dto from the inherited notification class
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetNotification>> GetAllNotifications(int userId)
        {
            return await _context.UserNotification
                .Include(n => n.notification)
                    .ThenInclude(n => n.personal) //Ez a szerző 
                        .ThenInclude(p => p.users)
                .Include(n => n.personal) //Ez a receiver
                .Where(n => n.UserId == userId).Select(n => new GetNotification(
                        n.notification.PublicId,
                        n.notification.personal.users.PublicId,
                        n.personal.users.PublicId,
                        n.notification.CreatedAt,
                        n.notification.Message,
                        n.IsRead,
                        n.notification.NotificationType,
                        HelperService.GetEnumDescription(n.notification.NotificationType),
                        n.personal.avatar
                    ))
                .ToListAsync();
        }
    }
}
