using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace KozoskodoAPI.Models
{
    /// <summary>
    /// Model of a Friendship
    /// </summary>
    [Table("Friend")]
    public partial class Friend
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "int(11)")]
        
        public int FriendshipID { get; set; }
        [Column(TypeName = "int(11)")]
        public int UserId { get; set; }
        [Column(TypeName = "int(11)")]
        public int FriendId { get; set; }
        [Column(TypeName = "int(11)")]
        public int StatusId { get; set; }
        public virtual ICollection<Personal>? GetPersonals { get; set; } = new HashSet<Personal>();
        public FriendshipStatus friendship_status { get; set; }
    }

    /// <summary>
    /// Reference table of Friend
    /// </summary>
    public partial class FriendshipStatus
    {
        [Key]
        [ForeignKey("friendship")]
        [Column(TypeName = "int(11)")]
        public int FK_Id { get; set; }
        [Column(TypeName = "int(11)")]
        public int Status { get; set; }
        public Friend friendship { get; set; }
    }

    public class Friend_notificationId : Friend
    {
        public int NotificationId { get; set; }
    }
}