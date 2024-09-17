using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozossegiAPI.Models
{
    [Table("Post")]
    [PrimaryKey(nameof(Id))]
    public partial class Post
    {
        [Key]
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SourceId { get; set; }
        public int? Likes { get; set; }
        public int? Dislikes { get; set; }
        public DateTime DateOfPost { get; set; } = DateTime.Now;
        [StringLength(500)]
        public string? PostContent { get; set; }
        public virtual ICollection<Comment> PostComments { get; set; } = new HashSet<Comment>();
        public virtual MediaContent? MediaContent { get; set; }
        public virtual ICollection<PostReaction>? PostReactions { get; set; } = new HashSet<PostReaction>();

        [JsonIgnore]
        public virtual ICollection<PersonalPost> PersonalPosts { get; set; } = new HashSet<PersonalPost>();
    }
}
