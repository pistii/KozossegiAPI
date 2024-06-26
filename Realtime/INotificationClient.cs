﻿using KozoskodoAPI.Models;

namespace KozoskodoAPI.Realtime
{
    public interface INotificationClient
    {
        Task ReceiveNotification(int userId, NotificationWithAvatarDto notificationDto);
        Task SendNotification(int userId, NotificationWithAvatarDto notificationDto);
    }
}
