using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public byte Loves { get; set; }
        public byte DisLoves { get; set; }
        public DateTime DateOfPost { get; set; }
        [StringLength(500)]
        public string? PostContent { get; set; }
        public int CommentId { get; set; }
        [ForeignKey("commentsId")]
        public virtual Comment? Comments { get; set; }
    }
}
