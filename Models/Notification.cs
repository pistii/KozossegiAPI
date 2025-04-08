using KozossegiAPI.Interfaces.Shared;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing.Text;
using System.Reflection;

namespace KozossegiAPI.Models
{
    public class Notification : IHasPublicId
    {
        public Notification()
        {
            this.ExpirationDate = NotificationExpirationCalculator.CalculateExpiration(this.NotificationType);
            this.PublicId = Guid.NewGuid().ToString("N");

        }
        public Notification(int authorId, string message, NotificationType type, string authorPublicId, string authorAvatar, int receiverUserId)
        {
            this.AuthorId = authorId;
            this.Message = message;
            this.NotificationType = type;
            this.ExpirationDate = NotificationExpirationCalculator.CalculateExpiration(type);
            this.PublicId = Guid.NewGuid().ToString("N");
            this.AuthorPublicId = authorPublicId;
            this.AuthorAvatar = authorAvatar;

            this.UserNotification.Add(new UserNotification(receiverUserId, this.Id, false));
        }

        public int Id { get; set; }
        public string PublicId { get; set; }
        public int AuthorId { get; set; }
        [ForeignKey(nameof(user.PublicId))]
        public string AuthorPublicId { get; set; }
        [StringLength(100)]
        public string AuthorAvatar { get; set; }
        [StringLength(300)]
        public string Message { get; set; }
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

        [Description("Friend Request Accepted")]
        FriendRequestRejected = 2,

        [Description("Happy Birthday!")]
        Birthday = 10,

        [Description("New Post Available")]
        NewPost = 20
    }


    /// <summary>
    /// Model to create connections.
    /// </summary>
    public class CreateNotification
    {
        public CreateNotification()
        {
            
        }

        public CreateNotification(int authorId, int userId, NotificationType notificationType, string message = "")
        {
            this.AuthorId = authorId;
            this.UserId = userId;
            this.Message = message;
            this.NotificationType = notificationType;
        }
        public int AuthorId { get; set; }
        public int UserId { get; set; }
        [StringLength(300)]
        public string? Message { get; set; }
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


        public GetNotification(Notification notification)
        {
            this.NotificationId = notification.PublicId;
            this.AuthorId = notification.AuthorPublicId;
            this.Avatar = notification.AuthorAvatar;
            this.Message = notification.Message;
            this.NotificationType = notification.NotificationType;
        }

        public GetNotification(Notification notification, Personal author)
        {
            this.NotificationId = notification.PublicId;
            this.AuthorId = author.users.PublicId;
            this.Avatar = author.avatar;
            this.Message = notification.Message;
            this.NotificationType = notification.NotificationType;
        }

        public GetNotification(string notificationId, string authorId, string receiverId, DateTime createdAt, string message, bool isRead, NotificationType notificationType, string notificationDescription, string avatar)
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
        public string NotificationId { get; set; }
        public string AuthorId { get; set; }
        public string ReceiverUserId { get; set; }
        [StringLength(300)]
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public NotificationType NotificationType { get; set; }
        public string notificationDescription { get; set; }
        public string? Avatar { get; set; }
    }

    public class NotificationExpirationCalculator
    {
        public NotificationExpirationCalculator()
        {
            
        }
        public static DateTime CalculateExpiration(NotificationType type)
        {
            return type switch
            {
                NotificationType.Birthday => DateTime.Now.AddDays(1),
                NotificationType.FriendRequest => DateTime.Now.AddDays(30),
                NotificationType.NewPost => DateTime.Now.AddHours(12),
                NotificationType.FriendRequestAccepted => DateTime.Now.AddHours(12),
                _ => DateTime.Now.AddDays(10)
            };
        }

    }
}