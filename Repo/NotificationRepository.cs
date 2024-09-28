using KozossegiAPI.Data;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Realtime;
using KozossegiAPI.Realtime.Connection;
using KozossegiAPI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Repo
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly DBContext _context;
        private readonly IFriendRepository _friendRepository;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IMapConnections _connections; 

        public NotificationRepository(DBContext context, 
            IFriendRepository friendRepository, 
            IHubContext<NotificationHub, INotificationClient> hub,
            IMapConnections mapConnections) : base(context)
        {
            _notificationHub = hub;
            _connections = mapConnections;
            _context = context;
            _friendRepository = friendRepository;
        }


        /// <summary>
        /// Searches the users who celebrates the birthday and sends a notification to the user's friends.
        /// </summary>
        /// <returns></returns>
        public async Task BirthdayNotification()
        {
            var birthdayUsers = await _friendRepository.GetAllUserWhoHasBirthdayToday();

            foreach (var person in birthdayUsers)
            {
                var friendIds = await _friendRepository.GetFriendIds(person.id);

                //Formázás
                string personName = HelperService.GetFullname(person.firstName, person.middleName, person.lastName);
                string birthdayContent = personName + " ma ünnepli a születésnapját.";

                foreach (var friend in friendIds)
                {
                    NotificationWithAvatarDto notification = new NotificationWithAvatarDto(friend, person.id, person.avatar, birthdayContent, NotificationType.Birthday);
                    Notification insert = notification;
                    await InsertSaveAsync(insert);
                    //Ha online a felhasználó akkor hubon keresztül értesítjük.
                    try
                    {
                        await RealtimeNotification(friend, notification);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                }
            }
        }

        public async Task RealtimeNotification(int toUserId, NotificationWithAvatarDto dto)
        {
            if (_connections.ContainsUser(toUserId))
                foreach (var user in _connections.GetConnectionsById(toUserId))
                {
                    await _notificationHub.Clients.Client(user).ReceiveNotification(toUserId, dto);
                }
        }

        public async Task SelectNotification()
        {
            var totalNotification = await GetDeletableNotifications();

            if (totalNotification.Count() > 1)
            {
                await RemoveNotifications(totalNotification);
                await SaveAsync();
            }
        }

        /// <summary>
        //  Search the Person's notifications, and create a new Dto from the inherited notification class
        /// </summary>
        /// <returns></returns>
        public async Task<List<NotificationWithAvatarDto>> GetAll_PersonNotifications(int userId)
        {
            return await _context.Notification
                .Where(_ => _.ReceiverId == userId).Select(n => new NotificationWithAvatarDto(n.ReceiverId, n.SenderId, "", n.notificationContent, n.notificationType)
                {
                    notificationId = n.notificationId,
                    notificationAvatar = _context.Personal.FirstOrDefault(p => p.id == n.ReceiverId).avatar,
                    createdAt = n.createdAt,
                    isNew = n.isNew
                }).ToListAsync();
        }

        public async Task RemoveNotifications(IEnumerable<Notification> entities)
        {
            _context.Notification.RemoveRange(entities);
        }

        /// <summary>
        /// deletes the notifications if older than 30 days. Runs in hosted services
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Notification>> GetDeletableNotifications()
        {
            DateTime now = DateTime.Now;
            var totalNotification = await _context.Notification.
                Where(n => n.createdAt <= now.AddDays(-30) && n.notificationType != NotificationType.FriendRequest ||
                //Also delete the notification if it was sent yesterday to greet the user
                n.createdAt <= now.AddDays(-1) && 
                n.notificationType == NotificationType.Birthday
                ).ToListAsync();
            return totalNotification;
        }

    }
}
