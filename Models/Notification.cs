using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    public class Notification
    {
        public Notification()
        {
            
        }

        public Notification(int personId, int notificationFromId, NotificationType type)
        {
            this.ReceiverId = personId;
            this.SenderId = notificationFromId;
            this.notificationType = type;
        }

        public int ReceiverId { get; set; }
        [Key]
        public int notificationId { get; set; }
        
        public int SenderId { get; set; }
        [StringLength(300)]
        public string notificationContent { get; set; } = string.Empty;
        public DateTime createdAt { get; set; } = DateTime.Now;
        public bool isNew { get; set; } = true;
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public NotificationType notificationType { get; set; } 

        [ForeignKey("ReceiverId")]
        [JsonIgnore]
        public virtual Personal? notification { get; set; }
    }

    public enum NotificationType
    {
        FriendRequest = 0,
        FriendRequestAccepted = 1,
        Birthday = 2,
        NewPost = 3,
        FriendRequestWithdraw = 4,
        FriendRequestReject = 5,
    }

    public class NotificationWithAvatarDto : Notification
    {
        public NotificationWithAvatarDto()
        {
            
        }
        public NotificationWithAvatarDto(int personId, int notificationFromId, string avatar, string notificationContent, NotificationType type) 
            : base(personId, notificationFromId, type)
        {
            this.ReceiverId = personId;
            this.SenderId = notificationFromId;
            this.notificationAvatar = avatar;
            this.notificationContent = notificationContent;
            this.notificationType = type;
        }

        public string? notificationAvatar { get; set; }
    }
}
