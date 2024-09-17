using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace KozossegiAPI.Models
{
   public partial class user
    {
        
        public user()
        {
            UserRestriction = new HashSet<UserRestriction>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "int(11)")]
        public int userID { get; set; }

        [StringLength(30)]
        [Required]
        [RegularExpression(@"^\w+([.-]?\w+)*@\w+([.-]?\w+)*(\.\w{2,3})+$", ErrorMessage = "Invalid secondary email format.")]
        public string email { get; set; }
        [StringLength(30)]
        [RegularExpression(@"^\w+([.-]?\w+)*@\w+([.-]?\w+)*(\.\w{2,3}|null)+$", ErrorMessage = "Invalid secondary email format.")]
        public string? SecondaryEmailAddress { get; set; }
        [StringLength(40)]
        [JsonIgnore]
        public string? password { get; set; }
        public DateTime? registrationDate { get; set; } = DateTime.Now;
        public virtual bool isActivated { get; set; } = false;
        [JsonIgnore]
        public string? Guid { get; set; }
        public DateTime LastOnline { get; set; }
        public bool isOnlineEnabled { get; set; }
        public virtual ICollection<UserRestriction> UserRestriction { get; }
        public virtual Personal? personal { get; set; }
        public virtual ICollection<Study>? Studies { get; set; }


        public user(user user)
        {
            this.email = user.email;
            this.personal = user.personal;
            this.SecondaryEmailAddress = user.SecondaryEmailAddress;
            this.userID = user.userID;
            this.isOnlineEnabled = user.isOnlineEnabled;
            this.LastOnline = user.LastOnline;
        }
    }
}