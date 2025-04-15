using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<GetNotification>> GetAllNotifications(int userId);
        Task UpdateNotification(CreateNotification cn);
        Task SendNotification(int receiverUserId, Personal author, CreateNotification createNotification);
        Task<Notification> CreateNotification(Personal author, CreateNotification cn);
    }
}
