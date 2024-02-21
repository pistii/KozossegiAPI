using Azure;

namespace KozoskodoAPI.Models
{
    public class UserRestriction
    {
        public int UserId { get; set; }
        public int RestrictionId { get; set; }

        public virtual user user { get; set; } = null!;
        public virtual Restriction restriction { get; set; } = null!;
    }
}
