using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace KozoskodoAPI.Models
{
    public partial class Settings
    {
        [Key]
        public int PK_Id { get; set; }
        [ForeignKey("personal")]
        public int FK_UserId { get; set; }
        public DateTime NextReminder { get; set; }
        [JsonIgnore]
        public virtual Personal? personal { get; set; } 
    }
}
