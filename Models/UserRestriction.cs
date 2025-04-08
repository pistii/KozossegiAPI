
using System.ComponentModel.DataAnnotations;

namespace KozossegiAPI.Models
{
    public class UserRestriction
    {
        [Key]
        public int UserId { get; set; }
        public int RestrictionId { get; set; }

        public virtual user user { get; set; } = null!;
        public virtual Restriction restriction { get; set; } = null!;
    }
}
