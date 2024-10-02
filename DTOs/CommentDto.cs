using KozossegiAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace KozossegiAPI.DTOs
{
    public class CommentDto
    {
        public CommentDto() {}
        public CommentDto(Comment comment)
        {
            CommentId = comment.commentId;
            CommentToken = comment.CommentToken;
            CommentDate = comment.CommentDate;
            CommentText = comment.CommentText;
            LastModified = comment.LastModified;
        }
        public int CommentId { get; set; }
        public DateTime CommentDate { get; set; } = DateTime.Now;
        [StringLength(36)]
        public string CommentToken { get; set; }
        public string CommentText { get; set; }
        public DateTime? LastModified { get; set; }
        public PersonalDto CommentAuthor { get; set; }
    }

    public class NewCommentDto
    {
        public NewCommentDto() {}
        public string postToken { get; set; }
        public int commenterId { get; set; }
        public string commentTxt { get; set; }
    }

    public class UpdateCommentDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "CommentToken field is required")]
        public string? CommentToken { get; set; }
        [Required(ErrorMessage = "CommenterId field is required")]
        public int commenterId { get; set; }
        public string? CommentTxt { get; set; }
    }

}
