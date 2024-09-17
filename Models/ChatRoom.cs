using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozossegiAPI.Models
{
    [Table("chatRoom")]
    public class ChatRoom
    {
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[JsonIgnore]
        [Key]
        public int chatRoomId { get; set; }
        public int senderId { get; set; }
        public int receiverId { get; set; }
        public DateTime? startedDateTime { get; set; } 
        public DateTime? endedDateTime { get; set; } = DateTime.Now;
        [JsonIgnore]
        public virtual ICollection<ChatContent> ChatContents { get; set; } = new HashSet<ChatContent>();

    }
}
