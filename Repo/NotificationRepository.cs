using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Repo
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly DBContext _context;

        public NotificationRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Notification> Get(int notificationId)
        {
            return await _context.Notification
                    .FirstOrDefaultAsync(x => x.notificationId == notificationId);
        }
        public Task BirthdayNotification()
        {
            throw new NotImplementedException();
        }

        public Task RealtimeNotification(int toUserId, NotificationWithAvatarDto dto)
        {
            throw new NotImplementedException();
        }

        public Task SelectNotification()
        {
            throw new NotImplementedException();
        }

        public async Task<Notification> GetNotification(Friend_notificationId friendship)
        {
            return await _context.Notification.FindAsync(friendship.NotificationId);
        }

        public async Task Update(Notification notification)
        {
            _context.Notification.Update(notification);
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



        public Task<List<Notification>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Notification> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(Notification entity)
        {
            await _context.Notification.AddAsync(entity);
        }

        public Task UpdateAsync(int id, Notification entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task RemoveNotifications(IEnumerable<Notification> entities)
        {
            _context.Notification.RemoveRange(entities);
        }

        /// <summary>
        /// deletes the notifications if older than 30 days. Runs in hosted services
        /// </summary>
        /// <returns></returns>
        public async Task<List<Notification>> DeletableNotifications()
        {
            DateTime now = DateTime.Now;
            var totalNotification = await _context.Notification.
                Where(n => EF.Functions.DateDiffDay(n.createdAt, now) >= 30 ||
                EF.Functions.DateDiffDay(n.createdAt, now) >= 1 && //Also delete the notification if it was sent yesterday to greet the user
                n.notificationType == NotificationType.Birthday
                ).ToListAsync();
            return totalNotification;
        }

        Task<IEnumerable<Notification>> INotificationRepository.DeletableNotifications()
        {
            throw new NotImplementedException();
        }
    }
}
