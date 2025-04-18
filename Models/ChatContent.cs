using KozossegiAPI.Interfaces.Shared;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace KozossegiAPI.Models
{
    [Table("chatContent")]
    public class ChatContent : IHasPublicId
    {
        public ChatContent()
        {
            sentDate = DateTime.Now;
            PublicId = Guid.NewGuid().ToString("N");
        }

        [Key]
        [ForeignKey("ChatFile")]
        public int MessageId { get; set; }
        public string PublicId { get; set; }
        public int AuthorId { get; set; }
        public int chatContentId { get; set; }
        [StringLength(800)]
        public string? message { get; set; } = null;
        public DateTime sentDate { get; set; }
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public Status status { get; set; }
        [JsonIgnore]
        public virtual ChatRoom? ChatRooms { get; set; }
        [JsonIgnore]
        public virtual ChatFile? ChatFile { get; set; }
        
        [JsonIgnore]
        [ForeignKey("AuthorId")]
        public virtual Personal? Author { get; set; }
    }

    public enum Status
    {
        Read = 0,
        Sent = 1,
        Delivered = 2
    }
}
