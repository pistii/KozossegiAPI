using KozossegiAPI.Models;

namespace KozossegiAPI.Realtime
{
    public interface INotificationClient
    {
        Task ReceiveNotification(int userId, GetNotification notificationDto);
        Task SendNotification(int userId, GetNotification notificationDto);
    }
}
