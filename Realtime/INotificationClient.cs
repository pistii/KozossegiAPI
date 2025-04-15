using KozossegiAPI.Models;

namespace KozossegiAPI.Realtime
{
    public interface INotificationClient
    {
        //Task ReceiveNotification(int userId, GetNotification notificationDto);
        //Task SendNotification(int userId, GetNotification notificationDto);
        Task SendNotification(int userId, object obj);

        Task ReceiveNotification(int userId, object obj);

        Task SendAsync(int userId, object obj);

    }
}
