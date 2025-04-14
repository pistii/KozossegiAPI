using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace KozossegiAPI.Models
{
    public class PostReaction
    {
        public PostReaction()
        {
            
        }
        public PostReaction(int postId, int userId, ReactionType reactionType)
        {
            this.PostId = postId;
            this.UserId = userId;
            this.ReactionTypeId = (int)reactionType;
        }

        [Key]
        public int Pk_Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public int ReactionTypeId { get; set; }
        [JsonIgnore]
        public Post post { get; set; }
        public ICollection<ReactionTypes> ReactionTypes { get; set; } = new HashSet<ReactionTypes>();
    }
}
