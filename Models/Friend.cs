using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace KozoskodoAPI.Models
{
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

        public virtual ICollection<Personal>? GetPersonals { get; set; } = new HashSet<Personal>();
    }

    public class Friend_notificationId : Friend
    {
        public int NotificationId { get; set; }
    }
}