using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace KozoskodoAPI.Models
{
    [Table("chatContent")]
    public class ChatContent
    {
        public int chatContentId { get; set; }
        [StringLength(800)]
        public string message { get; set; } = null!;
        public DateTime? sentDate { get; set; }
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
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
