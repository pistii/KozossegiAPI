using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    public class Notification
    {
        public int personId { get; set; }
        [Key]
        public int notificationId { get; set; }
        
        public int notificationFrom { get; set; }
        [StringLength(300)]
        public string notificationContent { get; set; } = string.Empty;
        public DateTime createdAt { get; set; } = DateTime.Now;
        public bool isNew { get; set; } = true;
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public NotificationType notificationType { get; set; } 

        [ForeignKey("personId")]
        [JsonIgnore]
        public virtual Personal? notification { get; set; }
    }

    public enum NotificationType
    {
        FriendRequest = 0,
        FriendRequestAccepted = 1,
        Birthday = 2,
        NewPost = 3
    }

    public class NotificationWithAvatarDto : Notification
    {
        public string notificationAvatar { get; set; } = null!;
    }
}
