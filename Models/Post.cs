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
        public Post()
        {
            
        }

        public Post(int PostId, string content, int likes, int dislikes)
        {
            Id = PostId;
            PostContent = content;
            Likes = likes;
            Dislikes = dislikes;
        }

        public Post(int PostId, string content, int likes, int dislikes, DateTime postDate)
        {
            Id = PostId;
            PostContent = content;
            Likes = likes;
            Dislikes = dislikes;
            DateOfPost = postDate;
        }

        [Key]
        [JsonIgnore]
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [StringLength(36)]
        public string Token { get; set; } = new Guid().ToString(); //Generates an empty guid in the next format: 00000000-0000-0000-0000-000000000000
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        public DateTime DateOfPost { get; set; } = DateTime.Now;
        [StringLength(500)]
        public string? PostContent { get; set; }
        [JsonIgnore]
        public virtual ICollection<Comment> PostComments { get; set; } = new HashSet<Comment>();
        [JsonIgnore]
        public virtual MediaContent? MediaContent { get; set; }
        public virtual ICollection<PostReaction>? PostReactions { get; set; } = new HashSet<PostReaction>();

        [JsonIgnore]
        public virtual ICollection<PersonalPost> PersonalPosts { get; set; } = new HashSet<PersonalPost>();
    }
}
