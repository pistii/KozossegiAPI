using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    public class Notification
    {
        public int personId { get; set; }
        [Key]
        public int notificationId { get; set; }
        [StringLength(70)]
        public string notificationFrom {  get; set; } = string.Empty;
        [StringLength(300)]
        public string notificationContent { get; set; } = string.Empty;
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public bool isReaded { get; set; } = false;

        [ForeignKey("personId")]
        [InverseProperty("Notifications")]
        [JsonIgnore]
        public virtual Personal? notification { get; set; }
    }
}
