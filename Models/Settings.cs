using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Models
{
    public partial class Settings
    {
        public Settings()
        {
            
        }

        public Settings(int userId, DateTime nextReminder, int postCreateEnabledToId = 1)
        {
            this.FK_UserId = userId;
            this.NextReminder = nextReminder;
            this.PostCreateEnabledToId = postCreateEnabledToId;
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_Id { get; set; }
        public int FK_UserId { get; set; }
        public DateTime NextReminder { get; set; } = DateTime.Now.AddDays(1);
        public int PostCreateEnabledToId { get; set; } = 1;

        [ForeignKey("FK_UserId")]
        [JsonIgnore]
        public virtual Personal? personal { get; set; } 
    }
}
