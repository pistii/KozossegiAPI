using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace KozossegiAPI.Models
{
    [Table("PersonalChatRoom")]
    public class PersonalChatRoom
    {
        [Key]
        public int Id { get; set; }
        public int FK_PersonalId { get; set; }
        public int FK_ChatRoomId { get; set; }

        [JsonIgnore]
        public virtual Personal PersonalRoom { get; set; }
    }
}
