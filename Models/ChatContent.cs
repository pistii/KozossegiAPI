using KozossegiAPI.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace KozoskodoAPI.Models
{
    [Table("chatContent")]
    public class ChatContent
    {
        public ChatContent()
        {
            
        }

        [Key]
        public int MessageId { get; set; }
        public int AuthorId { get; set; }
        public int chatContentId { get; set; }
        [StringLength(800)]
        public string message { get; set; } = null!;
        public DateTime? sentDate { get; set; } = DateTime.Now;
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public Status status { get; set; }
        [JsonIgnore]
        public virtual ChatRoom? ChatRooms { get; set; }
        [JsonIgnore]
        public virtual ChatFile? ChatFile { get; set; }
    }

    public enum Status
    {
        Read = 0,
        Sent = 1,
        Delivered = 2
    }
}
