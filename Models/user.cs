using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KozossegiAPI.Interfaces.Shared;
using Newtonsoft.Json;

namespace KozossegiAPI.Models
{
   public partial class user : IHasPublicId
    {
        
        public user()
        {
            UserRestriction = new HashSet<UserRestriction>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "int(11)")]
        [JsonIgnore]
        public int userID { get; set; }

        [StringLength(100)]
        [Required]
        [RegularExpression(@"^\w+([.-]?\w+)*@\w+([.-]?\w+)*(\.\w{2,3})+$", ErrorMessage = "Invalid email format.")]
        public string email { get; set; }
        [StringLength(30)]
        [RegularExpression(@"^\w+([.-]?\w+)*@\w+([.-]?\w+)*(\.\w{2,3}|null)+$", ErrorMessage = "Invalid secondary email format.")]
        public string? SecondaryEmailAddress { get; set; }
        [StringLength(40)]
        [JsonIgnore]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", ErrorMessage = "Invalid password format. Must contain at least one uppercase, one lowercase, one number.")]
        public string? password { get; set; }
        public DateTime? registrationDate { get; set; } = DateTime.Now;
        public virtual bool isActivated { get; set; } = false;
        public string PublicId { get; set; }
        public DateTime LastOnline { get; set; }
        public bool isOnlineEnabled { get; set; } = true;
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