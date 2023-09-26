using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    [Table("chatContent")]
    public class ChatContent
    {
        [JsonIgnore]
        public int chatContentId { get; set; }
        [StringLength(800)]
        public string message { get; set; } = null!;
        public DateTime? sentDate { get; set; }
        public Status status { get; set; }
        [JsonIgnore]
        public virtual ChatRoom? ChatRooms { get; set; }
    }

    public enum Status
    {
        Read = 0,
        Sent = 1,
        Delivered = 2
    }
}
