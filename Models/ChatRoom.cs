using KozossegiAPI.Interfaces.Shared;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozossegiAPI.Models
{
    [Table("chatRoom")]
    public class ChatRoom : IHasPublicId
    {
        public ChatRoom()
        {
            PublicId = Guid.NewGuid().ToString("N");
            endedDateTime = DateTime.Now;
        }
        
        [Key]
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int chatRoomId { get; set; }
        public string PublicId { get; set; }
        public string ReceiverPublicId { get; set; } = null!;
        public int senderId { get; set; }
        public int receiverId { get; set; }
        public DateTime? startedDateTime { get; set; } 
        public DateTime? endedDateTime { get; set; }
        [JsonIgnore]
        public virtual ICollection<ChatContent> ChatContents { get; set; } = new HashSet<ChatContent>();
        [ForeignKey("senderId")]
        public virtual Personal? Sender { get; set; }
        [ForeignKey("receiverId")]
        public virtual Personal? Author { get; set; }
    }
}
