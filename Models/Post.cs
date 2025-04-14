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
            this.Token = Guid.NewGuid().ToString();
        }

        public Post(string content)
        {
            this.PostContent = content;
            this.Token = Guid.NewGuid().ToString();
        }

        public Post(int PostId, string token, string content, int likes, int dislikes, DateTime postDate, DateTime? lastModified, MediaContent? mediaContent)
        {
            Id = PostId;
            Token = token;
            PostContent = content;
            Likes = likes;
            Dislikes = dislikes;
            DateOfPost = postDate;
            LastModified = lastModified;
            MediaContent = mediaContent;
        }

        [Key]
        [JsonIgnore]
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [StringLength(36)]
        public string Token { get; set; }
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        public DateTime DateOfPost { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
        [StringLength(500)]
        public string? PostContent { get; set; }
        [JsonIgnore]
        public virtual ICollection<Comment> PostComments { get; set; } = new HashSet<Comment>();
        public virtual MediaContent? MediaContent { get; set; }
        public virtual ICollection<PostReaction>? PostReactions { get; set; } = new HashSet<PostReaction>();

        [JsonIgnore]
        public virtual ICollection<PersonalPost> PersonalPosts { get; set; } = new HashSet<PersonalPost>();
    }
}
