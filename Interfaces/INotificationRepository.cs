using KozossegiAPI.Models;

namespace KozossegiAPI.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<GetNotification>> GetAllNotifications(int userId);
        Task SendNotification(int receiverUserId, Personal author, CreateNotification createNotification);
    }
}
