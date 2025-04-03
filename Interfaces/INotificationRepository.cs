using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task RealtimeNotification(int toUserId, GetNotification dto);
        Task BirthdayNotification();
        Task SelectNotification();
        Task<List<GetNotification>> GetAllNotifications(int userId);
        Task RemoveNotifications(IEnumerable<Notification> entity);
        Task<IEnumerable<Notification>> GetDeletableNotifications();

        Task CreateNotification(CreateNotification notification);

    }
}
