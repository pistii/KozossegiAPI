using KozossegiAPI.Models;

namespace KozossegiAPI.Realtime
{
    public interface INotificationClient
    {
        Task ReceiveNotification(int userId, NotificationWithAvatarDto notificationDto);
        Task SendNotification(int userId, NotificationWithAvatarDto notificationDto);
    }
}
