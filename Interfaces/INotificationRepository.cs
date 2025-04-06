using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task RealtimeNotification(int toUserId, GetNotification dto);
        Task<List<GetNotification>> GetAllNotifications(int userId);
        Task<Notification> CreateNotification(CreateNotification cn);
        Task UpdateNotification(CreateNotification cn);
        Task<Notification> CreateFriendRequestAcceptedNotification(CreateNotification createNotification);
    }
}
