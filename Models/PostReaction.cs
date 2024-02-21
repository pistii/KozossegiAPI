using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace KozoskodoAPI.Models
{
    public class PostReaction
    {
        [Key]
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_AI { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string? ReactionType { get; set; }
        [JsonIgnore]
        public Post post { get; set; }
    }
}
