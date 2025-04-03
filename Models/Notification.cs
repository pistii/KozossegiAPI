using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace KozossegiAPI.Models
{
    public class Notification
    {
        public Notification()
        {
            
        }
        public Notification(int authorId, string message, NotificationType type)
        {
            this.AuthorId = authorId;
            this.Message = message;
            this.NotificationType = type;
        }

        public int Id { get; set; }
        public int AuthorId { get; set; }
        [StringLength(300)]
        public string Message {  get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ExpirationDate { get; set; }
        public NotificationType NotificationType { get; set; }
        [ForeignKey("AuthorId")]
        [JsonIgnore]
        public virtual Personal? personal { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserNotification>? UserNotification { get; set; } = new HashSet<UserNotification>();

    }

    public enum NotificationType
    {
        [Description("Friend Request Sent")]
        FriendRequest = 0,

        [Description("Friend Request Accepted")]
        FriendRequestAccepted = 1,

        [Description("Happy Birthday!")]
        Birthday = 2,

        [Description("New Post Available")]
        NewPost = 3
    }


    /// <summary>
    /// Model to create connections.
    /// </summary>
    public class CreateNotification
    {
        public int AuthorId { get; set; }
        public int UserId { get; set; }
        [StringLength(300)]
        public string Message { get; set; }
        public NotificationType NotificationType { get; set; }
    }

    /// <summary>
    /// Model returned to user in response.
    /// </summary>
    public class GetNotification
    {
        public GetNotification()
        {
            
        }

        public GetNotification(int notificationId, int authorId, int receiverId, DateTime createdAt, string message, bool isRead, NotificationType notificationType, string notificationDescription, string avatar)
        {
            this.NotificationId = notificationId;
            this.AuthorId = authorId;
            this.ReceiverUserId = receiverId;
            this.CreatedAt = createdAt;
            this.Message = message;
            this.IsRead = isRead;
            this.NotificationType = notificationType;
            this.notificationDescription = notificationDescription;
            this.Avatar = avatar;
        }
        public int NotificationId { get; set; }
        public int AuthorId { get; set; }
        public int ReceiverUserId { get; set; }
        [StringLength(300)]
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public NotificationType NotificationType { get; set; }
        public string notificationDescription { get; set; }
        public string? Avatar { get; set; }
    }
}
