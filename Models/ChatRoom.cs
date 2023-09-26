using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KozoskodoAPI.Models
{
    [Table("chatRoom")]
    public class ChatRoom
    {
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[JsonIgnore]
        public int chatRoomId { get; set; }
        public int senderId { get; set; }
        public int receiverId { get; set; }
        public DateTime? startedDateTime { get; set; }
        public DateTime? endedDateTime { get; set; }

        [ForeignKey("chatContentId")]
        public virtual ICollection<ChatContent> ChatContents { get; set; } = new HashSet<ChatContent>();

    }
}
