using KozoskodoAPI.Models;
using KozosKodoAPI.Repo;

namespace KozoskodoAPI.Repo
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<Notification> Get(int notificationId);
        Task RealtimeNotification(int toUserId, NotificationWithAvatarDto dto);
        Task BirthdayNotification();
        Task SelectNotification();
        Task<Notification> GetNotification(Friend_notificationId notificationId);
        Task Update(Notification notification);
        Task<List<NotificationWithAvatarDto>> GetAll_PersonNotifications(int userId);
        Task SaveAsync();
        Task RemoveNotifications(IEnumerable<Notification> entity);

        Task<IEnumerable<Notification>> DeletableNotifications();
    }
}
