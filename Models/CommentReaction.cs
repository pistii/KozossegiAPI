using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace KozossegiAPI.Models
{
    public class CommentReaction
    {
        public CommentReaction()
        {

        }
        public CommentReaction(int commentId, int userId, ReactionType reactionType)
        {
            this.FK_CommentId = commentId;
            this.FK_UserId = userId;
            this.ReactionTypeId = (int)reactionType;
        }

        [Key]
        public int Pk_Id { get; set; }
        public int FK_CommentId { get; set; }
        public int FK_UserId { get; set; }
        public int ReactionTypeId { get; set; }
        [JsonIgnore]
        public Comment Comment { get; set; }
        public ICollection<ReactionTypes> ReactionTypes { get; set; } = new HashSet<ReactionTypes>();
    }
}
