using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task RealtimeNotification(int toUserId, NotificationWithAvatarDto dto);
        Task BirthdayNotification();
        Task SelectNotification();
        Task<List<NotificationWithAvatarDto>> GetAll_PersonNotifications(int userId);
        Task RemoveNotifications(IEnumerable<Notification> entity);
        Task<IEnumerable<Notification>> GetDeletableNotifications();
    }
}
