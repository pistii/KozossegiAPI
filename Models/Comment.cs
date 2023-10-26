using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    [Table("Comment")]
    public partial class Comment
    {
        [Key]
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int commentId { get; set; }
        public int PostId { get; set; }
        public int FK_AuthorId { get; set; }
        public DateTime CommentDate { get; set; }
        [StringLength(500)]
        public string? CommentText { get; set; }
        [JsonIgnore]
        public Post Post { get; set; }
        //[JsonIgnore]
        //public virtual ICollection<Post> Post { get; set; } = new HashSet<Post>();
    }
}
