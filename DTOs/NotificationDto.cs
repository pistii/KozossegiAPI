using KozoskodoAPI.Models;

namespace KozoskodoAPI.DTOs
{
    public class NotificationDto //store notificationId?
    {
        public int applicantId {  get; set; }
        public int toUserId { get; set; }
        public string content { get; set; } = string.Empty;
        public NotificationType notificationType { get; set; }
        //TODO: egyszerűsíteni az értesítéseket, -> classok minimalizálása
    }
}
