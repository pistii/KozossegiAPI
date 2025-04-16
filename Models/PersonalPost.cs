using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KozossegiAPI.Models
{
    [Table("PersonalPost")]
    public class PersonalPost
    {
        [Key]
        public int PersonalPostId { get; set; }
        public int AuthorId { get; set; }
        public int PostedToId { get; set; }
        public int PostId { get; set; }
        [ForeignKey("AuthorId")]
        public virtual Personal Author { get; set; }
        [ForeignKey("PostedToId")]
        public virtual Personal Receiver { get; set; }
        public virtual Post Posts { get; set; }
    }
}
