using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace KozoskodoAPI.Models
{
    public partial class Comment
    {
        [Key]
        public int Id { get; set; }
        public DateTime CommentDate { get; set; }
        [StringLength(500)]
        public string? CommentText { get; set; }
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual ICollection<Post> comments { get; set; } = new HashSet<Post>();
    }
}
