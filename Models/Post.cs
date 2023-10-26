using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    [Table("Post")]
    [PrimaryKey(nameof(Id))]
    public partial class Post
    {
        [Key]
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public byte? Loves { get; set; }
        public byte? DisLoves { get; set; }
        public DateTime DateOfPost { get; set; }
        [StringLength(500)]
        public string? PostContent { get; set; }
        public virtual ICollection<Comment> PostComments { get; set; } = new HashSet<Comment>();
        [JsonIgnore]
        public virtual ICollection<PersonalPost> PersonalPosts { get; set; } = new HashSet<PersonalPost>();
    }
}
