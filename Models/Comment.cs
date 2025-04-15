using KozossegiAPI.Interfaces.Shared;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozossegiAPI.Models
{
    [Table("Comment")]
    public partial class Comment : IHasPublicId
    {
        [Key]
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int commentId { get; set; }
        [JsonIgnore]
        public int PostId { get; set; }
        [JsonIgnore]
        public int FK_AuthorId { get; set; }
        [StringLength(36)]
        public string PublicId { get; set; }
        public DateTime CommentDate { get; set; }
        [StringLength(500)]
        public string? CommentText { get; set; }
        public DateTime? LastModified { get; set; }
        [JsonIgnore]
        public Post Post { get; set; }
        [ForeignKey("FK_AuthorId")]
        public Personal AuthorPerson { get; set; }
        public virtual ICollection<CommentReaction>? CommentReactions { get; set; } = new HashSet<CommentReaction>();
    }
}
