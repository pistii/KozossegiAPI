using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KozoskodoAPI.Models
{
    [Table("PersonalPost")]
    public class PersonalPost
    {
        [Key]
        public int personalPostId { get; set; }
        public int personId { get; set; }
        public int postId { get; set; }
        public virtual Personal Personal_posts { get; set; }
        public virtual Post Posts { get; set; }
    }
}
